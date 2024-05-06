using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Installer.Properties;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Installer;

internal sealed class InstallCommand : AsyncCommand<InstallCommand.Settings>
{
	internal class Settings : CommandSettings
	{
		[Description("Path to EFT.")]
		[CommandArgument(0, "[path]")]
		public string? Path { get; set; }

		[Description("Use specific trainer branch version.")]
		[CommandOption("-b|--branch")]
		public string? Branch { get; set; }

		[Description("Disable feature.")]
		[CommandOption("-f|--feature")]
		public string[]? DisabledFeatures { get; set; }

		[Description("Disable command.")]
		[CommandOption("-c|--command")]
		public string[]? DisabledCommands { get; set; }
	}

	public static string[] ToSourceFile(string[]? names, string folder)
	{
		names ??= [];
		return names
			.Select(f => $"{folder}\\{f}.cs")
			.ToArray();
	}

	[SupportedOSPlatform("windows")]
	public override async Task<int> ExecuteAsync(CommandContext commandContext, Settings settings)
	{
		try
		{
			AnsiConsole.MarkupLine("-=[[ [cyan]EscapeFromTarkov-Trainer Universal Installer[/] - [blue]https://github.com/sailro [/]]]=-");
			AnsiConsole.WriteLine();

			var installation = Installation.GetTargetInstallation(settings.Path, "Please select where to install the trainer");
			if (installation == null)
				return (int)ExitCode.NoInstallationFound;

			AnsiConsole.MarkupLine($"Target [green]EscapeFromTarkov ({installation.Version})[/] in [blue]{installation.Location.EscapeMarkup()}[/].");

			const string features = "Features";
			const string commands = "ConsoleCommands";

			settings.DisabledFeatures = ToSourceFile(settings.DisabledFeatures, features);
			settings.DisabledCommands = ToSourceFile(settings.DisabledCommands, commands);

			var (compilation, archive) = await BuildTrainerAsync(settings, installation, features, commands);

			if (compilation == null)
			{
				// Failure
				AnsiConsole.MarkupLine($"[red]Unable to compile trainer for version {installation.Version}. Please file an issue here : https://github.com/sailro/EscapeFromTarkov-Trainer/issues [/]");
				return (int)ExitCode.CompilationFailed;
			}

			if (installation.UsingSptAki)
			{
				AnsiConsole.MarkupLine("[green][[SPT-AKI]][/] detected. Please make sure you have run the game at least once before installing the trainer.");
				AnsiConsole.MarkupLine("SPT-AKI is patching binaries during the first run, and we [underline]need[/] to compile against those patched binaries.");
				AnsiConsole.MarkupLine("If you install this trainer on stock binaries, the game will freeze at the startup screen.");

				if (!AnsiConsole.Confirm("Continue installation (yes I have run the game at least once) ?"))
					return (int)ExitCode.Canceled;
			}

			if (!CreateDll(installation, "NLog.EFT.Trainer.dll", dllPath => compilation.Emit(dllPath)))
				return (int)ExitCode.CreateDllFailed;

			if (!CreateDll(installation, "0Harmony.dll", dllPath => File.WriteAllBytes(dllPath, Resources._0Harmony), false))
				return (int)ExitCode.CreateHarmonyDllFailed;

			if (!CreateOutline(installation, archive!))
				return (int)ExitCode.CreateOutlineFailed;

			const string bepInExPluginProject = "BepInExPlugin.csproj";
			if (installation.UsingBepInEx && archive!.Entries.Any(e => e.Name == bepInExPluginProject))
			{
				AnsiConsole.MarkupLine("[green][[BepInEx]][/] detected. Creating plugin instead of using NLog configuration.");

				// reuse successful context for compiling.
				var pluginContext = new CompilationContext(installation, "plugin", bepInExPluginProject)
				{
					Archive = archive, 
					Branch = GetInitialBranch(settings)
				};
				var (pluginCompilation, _, _) = await GetCompilationAsync(pluginContext);

				if (pluginCompilation == null)
				{
					AnsiConsole.MarkupLine($"[red]Unable to compile plugin for version {installation.Version}. Please file an issue here : https://github.com/sailro/EscapeFromTarkov-Trainer/issues [/]");
					return (int)ExitCode.PluginCompilationFailed;
				}

				if (!CreateDll(installation, Path.Combine(installation.BepInExPlugins, "aki-efttrainer.dll"), dllPath => pluginCompilation.Emit(dllPath)))
					return (int)ExitCode.CreatePluginDllFailed;
			}
			else
			{
				var version = new Version(0, 13, 0, 21531);
				if (installation.Version >= version)
				{
					AnsiConsole.MarkupLine($"[yellow]Warning: EscapeFromTarkov {version} or later prevent this trainer to be loaded using NLog configuration.[/]");
					AnsiConsole.MarkupLine("[yellow]It is now mandatory to use SPT-AKI/BepInEx, or to find your own way to load the trainer. As is, it will not work.[/]");
				}

				CreateOrPatchConfiguration(installation);
			}

			TryCreateGameDocumentFolder();
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Error: {ex.Message.EscapeMarkup()}. Please file an issue here : https://github.com/sailro/EscapeFromTarkov-Trainer/issues [/]");
			return (int)ExitCode.Failure;
		}

		return (int)ExitCode.Success;
	}

	private static async Task<(CSharpCompilation?, ZipArchive?)> BuildTrainerAsync(Settings settings, Installation installation, params string[] folders)
	{
		// Try first to compile against master
		var context = new CompilationContext(installation, "trainer", "NLog.EFT.Trainer.csproj")
		{
			Exclude = [.. settings.DisabledFeatures!, .. settings.DisabledCommands!],
			Branch = GetInitialBranch(settings)
		};

		var (compilation, archive, errors) = await GetCompilationAsync(context);
		var files = errors
			.Select(d => d.Location.SourceTree?.FilePath)
			.Where(s => s is not null)
			.Distinct()
			.ToArray();

		if (compilation == null)
		{
			// Failure, so try with a dedicated branch if exists
			var retryBranch = GetRetryBranch(installation, context);
			if (retryBranch != null)
			{
				context.Branch = retryBranch;
				(compilation, archive, _) = await GetCompilationAsync(context);
			}
		}

		if (compilation == null && files.Length != 0 && files.All(file => folders.Any(folder => file!.StartsWith(folder))))
		{
			// Failure, retry by removing faulting features if possible
			AnsiConsole.MarkupLine($"[yellow]Trying to disable faulting feature/command: [red]{GetFaultingNames(files!)}[/].[/]");

			context.Exclude = [.. files!, .. settings.DisabledFeatures!, .. settings.DisabledCommands!];
			context.Branch = GetFallbackBranch(settings);

			(compilation, archive, errors) = await GetCompilationAsync(context);

			if (errors.Length == 0)
				AnsiConsole.MarkupLine("[yellow]We found a fallback! But please file an issue here : https://github.com/sailro/EscapeFromTarkov-Trainer/issues [/]");
		}

		return (compilation, archive);
	}

	private static string GetFaultingNames(string[] files)
	{
		return string.Join(", ", files
			.Select(Path.GetFileNameWithoutExtension)
			.Where(f => !f!.StartsWith("Base"))
			.Distinct()
			.OrderBy(f => f));
	}

	private static string GetDefaultBranch()
	{
		return CompilationContext.DefaultBranch;
	}

	private static string GetInitialBranch(Settings settings)
	{
		return settings.Branch ?? GetDefaultBranch();
	}

	private static string? GetRetryBranch(Installation installation, CompilationContext context)
	{
		var dedicated = "dev-" + installation.Version;
		return dedicated == context.Branch ? null : dedicated; // no need to reuse the same initial branch for a retry
	}

	private static string GetFallbackBranch(Settings settings)
	{
		return GetInitialBranch(settings);
	}

	private static void TryCreateGameDocumentFolder()
	{
		var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Escape from Tarkov");
		if (Directory.Exists(folder))
			return;

		try
		{
			Directory.CreateDirectory(folder);
			AnsiConsole.MarkupLine($"Created [blue]{folder.EscapeMarkup()}[/] folder.");
		}
		catch (Exception)
		{
			AnsiConsole.MarkupLine($"[yellow]Unable to create [blue]{folder.EscapeMarkup()}[/]. We need this folder to store our [green]trainer.ini[/] later.[/]");
		}
	}

	private static async Task<(CSharpCompilation?, ZipArchive?, Diagnostic[])> GetCompilationAsync(CompilationContext context)
	{
		var errors = Array.Empty<Diagnostic>();

		var archive = context.Archive ?? await GetSnapshotAsync(context.Try, context.Branch);
		if (archive == null)
		{
			context.Try++;
			return (null, null, errors);
		}

		CSharpCompilation? compilation = null;
		AnsiConsole
			.Status()
			.Start($"Compiling {context.ProjectTitle}", _ =>
			{
				var compiler = new Compiler(archive, context);
				compilation = compiler.Compile(Path.GetFileNameWithoutExtension(context.Project));
				errors = compilation
					.GetDiagnostics()
					.Where(d => d.Severity == DiagnosticSeverity.Error)
					.ToArray();

#if DEBUG
				foreach (var error in errors)
					AnsiConsole.MarkupLine($"[grey]>> {error.Id} [[{error.Location.SourceTree?.FilePath.EscapeMarkup()}]]: {error.GetMessage().EscapeMarkup()}.[/]");
#endif

				if (errors.Length != 0)
				{
					AnsiConsole.MarkupLine($">> [blue]Try #{context.Try}[/] [yellow]Compilation failed for {context.Branch.EscapeMarkup()} branch.[/]");
					compilation = null;
				}
				else
				{
					AnsiConsole.MarkupLine($">> [blue]Try #{context.Try}[/] Compilation [green]succeed[/] for [blue]{context.Branch.EscapeMarkup()}[/] branch.");
				}
			});

		context.Try++;
		return (compilation, archive, errors);
	}

	private static async Task<ZipArchive?> GetSnapshotAsync(int @try, string branch)
	{
		var status = $"Downloading repository snapshot ({branch} branch)...";
		ZipArchive? result = null;

		try
		{
			var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, certificate, chain, errors) =>
				{
					if (errors == SslPolicyErrors.None)
						return true;

					AnsiConsole.MarkupLine($"[yellow]Warning: [[{errors}]] We found SSL issues.[/]");
					foreach (var chainStatus in chain?.ChainStatus ?? [])
						AnsiConsole.MarkupLine($"[yellow]Warning: [[{chainStatus.Status}]] {chainStatus.StatusInformation.EscapeMarkup()}[/]");

					if (certificate == null) 
						return true;

					AnsiConsole.MarkupLine($"[yellow]Warning: [[Subject]] {certificate.Subject.EscapeMarkup()}.[/]");
					AnsiConsole.MarkupLine($"[yellow]Warning: [[Issuer]] {certificate.Issuer.EscapeMarkup()}.[/]");

					return true;
				}
			};

			await AnsiConsole
				.Status()
				.StartAsync(status, async _ =>
				{
					using var client = new HttpClient(handler);
					var buffer = await client.GetByteArrayAsync(new Uri($"https://github.com/sailro/EscapeFromTarkov-Trainer/archive/refs/heads/{branch}.zip"));
					var stream = new MemoryStream(buffer);
					result = new ZipArchive(stream, ZipArchiveMode.Read);
				});
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine(ex is HttpRequestException {StatusCode: HttpStatusCode.NotFound} ? $">> [blue]Try #{@try}[/] [yellow]Branch {branch.EscapeMarkup()} not found.[/]" : $"[red]Error: {ex.Message.EscapeMarkup()}[/]");
		}

		return result;
	}

	private static void CreateOrPatchConfiguration(Installation installation)
	{
		const string targetName = "EFTTarget";
		var configPath = Path.Combine(installation.Managed, "NLog.dll.nlog");
		try
		{
			if (File.Exists(configPath))
			{
				var doc = new XmlDocument();
				doc.Load(configPath);

				var nlogNode = doc.DocumentElement;
				var targetsNode = nlogNode?.FirstChild;

				if (nlogNode is not {Name: "nlog"} || targetsNode is not {Name: "targets"})
				{
					AnsiConsole.MarkupLine($"[red]Unable to patch {configPath.EscapeMarkup()}, unexpected xml structure.[/]");
					return;
				}

				if (targetsNode.ChildNodes.Cast<XmlNode>().Any(targetNode => targetNode.Attributes?["name"]?.Value == targetName && targetNode.Attributes?["xsi:type"]?.Value == targetName))
				{
					AnsiConsole.MarkupLine($"Already patched [green]{Path.GetFileName(configPath).EscapeMarkup()}[/] in [blue]{Path.GetDirectoryName(configPath).EscapeMarkup()}[/].");
					return;
				}

				var entry = doc.CreateElement("target");
				var name = doc.CreateAttribute("name");
				name.Value = targetName;
				var type = doc.CreateAttribute("xsi", "type", "http://www.w3.org/2001/XMLSchema-instance");
				type.Value = targetName;
				entry.Attributes.Append(name);
				entry.Attributes.Append(type);
				targetsNode.AppendChild(entry);

				var builder = new StringBuilder();
				using var writer = new UTF8StringWriter(builder);
				doc.Save(writer);
				builder.Replace(" xmlns=\"\"", string.Empty);
				File.WriteAllText(configPath, builder.ToString());

				AnsiConsole.MarkupLine($"Patched [green]{Path.GetFileName(configPath).EscapeMarkup()}[/] in [blue]{Path.GetDirectoryName(configPath).EscapeMarkup()}[/].");
				return;
			}

			var content = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<nlog xmlns=""http://www.nlog-project.org/schemas/NLog.xsd"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <targets>
    <target name=""{targetName}"" xsi:type=""{targetName}"" />
  </targets>
</nlog>";
			File.WriteAllText(configPath, content);
			AnsiConsole.MarkupLine($"Created [green]{Path.GetFileName(configPath).EscapeMarkup()}[/] in [blue]{Path.GetDirectoryName(configPath).EscapeMarkup()}[/].");
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Unable to patch or create {configPath.EscapeMarkup()}: {ex.Message.EscapeMarkup()}.[/]");
		}
	}

	private static bool CreateOutline(Installation installation, ZipArchive archive)
	{
		var outlinePath = Path.Combine(installation.Data, "outline");
		try
		{
			var entry = archive.Entries.FirstOrDefault(e => e.Name.Equals(Path.GetFileName(outlinePath), StringComparison.OrdinalIgnoreCase));
			if (entry == null)
			{
				AnsiConsole.MarkupLine("[red]Unable to find outline in the zip archive.[/]");
				return false;
			}

			using var input = entry.Open();
			using var output = File.Create(outlinePath);
			input.CopyTo(output);

			AnsiConsole.MarkupLine($"Created [green]{Path.GetFileName(outlinePath).EscapeMarkup()}[/] in [blue]{Path.GetDirectoryName(outlinePath).EscapeMarkup()}[/].");
			return true;
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Unable to create {outlinePath.EscapeMarkup()}: {ex.Message.EscapeMarkup()}.[/]");
			return false;
		}
	}

	private static bool CreateDll(Installation installation, string filename, Action<string> creator, bool overwrite = true)
	{
		var dllPath = Path.IsPathRooted(filename) ? filename : Path.Combine(installation.Managed, filename);
		var dllPathBepInExCore = Path.IsPathRooted(filename) ? null : Path.Combine(installation.BepInExCore, filename);

		try
		{
			// Check for prerequisites, already provided by BepInEx
			if (dllPathBepInExCore != null && File.Exists(dllPathBepInExCore))
				return true;

			if (!overwrite && File.Exists(dllPath))
				return true;

			creator(dllPath);
			AnsiConsole.MarkupLine($"Created [green]{Path.GetFileName(dllPath).EscapeMarkup()}[/] in [blue]{Path.GetDirectoryName(dllPath).EscapeMarkup()}[/].");

			return true;
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Unable to create {dllPath.EscapeMarkup()}: {ex.Message.EscapeMarkup()} [/]");
			return false;
		}
	}
}
