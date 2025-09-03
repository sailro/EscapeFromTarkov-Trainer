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
using Microsoft.CodeAnalysis.Emit;
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

		[Description("Language.")]
		[CommandOption("-l|--language")]
		public string Language { get; set; } = "";
	}

	public static string[] ToSourceFile(string[]? names, string folder)
	{
		names ??= [];
		return [.. names.Select(f => $"{folder}\\{f}.cs")];
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

			if (installation.UsingSpt)
			{
				AnsiConsole.MarkupLine("[green][[SPT]][/] detected. Please make sure you have run the game at least once before installing the trainer.");
				AnsiConsole.MarkupLine("SPT is patching binaries during the first run, and we [underline]need[/] to compile against those patched binaries.");
				AnsiConsole.MarkupLine("If you install this trainer on stock binaries, we'll be unable to compile or the game will freeze at the startup screen.");

				if (installation.UsingSptButNeverRun)
					AnsiConsole.MarkupLine("[yellow]Warning: it seems that you have never run your SPT installation. You should quit now and rerun this installer once it's done.[/]");

				if (!await AnsiConsole.ConfirmAsync("Continue installation (yes I have run the game at least once) ?"))
					return (int)ExitCode.Canceled;
			}

			const string features = "Features";
			const string commands = "ConsoleCommands";

			settings.DisabledFeatures = ToSourceFile(settings.DisabledFeatures, features);
			settings.DisabledCommands = ToSourceFile(settings.DisabledCommands, commands);

			var result = await BuildTrainerAsync(settings, installation, features, commands);

			if (result.Compilation == null)
			{
				// Failure
				AnsiConsole.MarkupLine($"[red]Unable to compile trainer for version {installation.Version}. Please file an issue here : https://github.com/sailro/EscapeFromTarkov-Trainer/issues [/]");
				return (int)ExitCode.CompilationFailed;
			}

			if (!CreateDll(installation, "NLog.EFT.Trainer.dll", dllPath => result.Compilation.Emit(dllPath, manifestResources: result.Resources)))
				return (int)ExitCode.CreateDllFailed;

			if (!CreateDll(installation, "0Harmony.dll", dllPath => File.WriteAllBytes(dllPath, Resources._0Harmony), false))
				return (int)ExitCode.CreateHarmonyDllFailed;

			if (!CreateOutline(installation, result.Archive!))
				return (int)ExitCode.CreateOutlineFailed;

			const string bepInExPluginProject = "BepInExPlugin.csproj";
			if (installation.UsingBepInEx && result.Archive!.Entries.Any(e => e.Name == bepInExPluginProject))
			{
				AnsiConsole.MarkupLine("[green][[BepInEx]][/] detected. Creating plugin instead of using NLog configuration.");

				// reuse successful context for compiling.
				var pluginContext = new CompilationContext(installation, "plugin", bepInExPluginProject)
				{
					Archive = result.Archive,
					Branch = GetInitialBranch(settings)
				};
				var pluginResult = await GetCompilationAsync(pluginContext);

				if (pluginResult.Compilation == null)
				{
					AnsiConsole.MarkupLine($"[red]Unable to compile plugin for version {installation.Version}. Please file an issue here : https://github.com/sailro/EscapeFromTarkov-Trainer/issues [/]");
					return (int)ExitCode.PluginCompilationFailed;
				}

				if (!CreateDll(installation, Path.Combine(installation.BepInExPlugins, "spt-efttrainer.dll"), dllPath => pluginResult.Compilation.Emit(dllPath)))
					return (int)ExitCode.CreatePluginDllFailed;
			}
			else
			{
				var version = new Version(0, 13, 0, 21531);
				if (installation.Version >= version)
				{
					AnsiConsole.MarkupLine($"[yellow]Warning: EscapeFromTarkov {version} or later prevent this trainer to be loaded using NLog configuration.[/]");
					AnsiConsole.MarkupLine("[yellow]It is now mandatory to use SPT/BepInEx, or to find your own way to load the trainer. As is, it will not work.[/]");
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

	private static async Task<CompilationResult> BuildTrainerAsync(Settings settings, Installation installation, params string[] folders)
	{
		// Try first to compile against master
		var context = new CompilationContext(installation, "trainer", "NLog.EFT.Trainer.csproj")
		{
			Exclude = [.. settings.DisabledFeatures!, .. settings.DisabledCommands!],
			Branch = GetInitialBranch(settings),
			Defines = installation.UsingSpt ? [] : ["EFT_LIVE"],
			Language = settings.Language
		};

		var result = await GetCompilationAsync(context);
		if (context.IsFatalFailure)
			return result;

		if (result.Compilation == null)
		{
			// Failure, so try with a dedicated branch if exists
			var retryBranch = GetRetryBranch(installation, context);
			if (retryBranch != null)
			{
				context.Branch = retryBranch;
				result = await GetCompilationAsync(context);
			}
		}

		var files = result.ErrorFiles;
		if (!HasFaultingFeatureOrCommand(result, folders, files))
			return result;

		// Failure, retry by removing faulting features if possible
		AnsiConsole.MarkupLine($"[yellow]Trying to disable faulting feature/command: [red]{GetFaultingNames(files)}[/].[/]");

		context.Exclude = [.. files, .. settings.DisabledFeatures, .. settings.DisabledCommands];
		context.Branch = GetFallbackBranch(settings);

		result = await GetCompilationAsync(context);

		if (result.Errors.Length == 0)
			AnsiConsole.MarkupLine("[yellow]We found a fallback! But please file an issue here : https://github.com/sailro/EscapeFromTarkov-Trainer/issues [/]");

		return result;
	}

	private static bool HasFaultingFeatureOrCommand(CompilationResult result, string[] folders, string[] files)
	{
		return result.Compilation == null && files.Length != 0 && files.All(file => folders.Any(file.StartsWith));
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

	private static async Task<CompilationResult> GetCompilationAsync(CompilationContext context)
	{
		var errors = Array.Empty<Diagnostic>();
		ResourceDescription[] resources = [];

		var archive = context.Archive ?? await GetSnapshotAsync(context, context.Branch);
		if (archive == null)
		{
			context.Try++;
			return new(null, null, errors, resources);
		}

		CSharpCompilation? compilation = null;

		AnsiConsole
			.Status()
			.Start($"Compiling {context.ProjectTitle}", _ =>
			{
				var compiler = new Compiler(archive, context);
				compilation = compiler.Compile(Path.GetFileNameWithoutExtension(context.Project));
				errors = [.. compilation
					.GetDiagnostics()
					.Where(d => d.Severity == DiagnosticSeverity.Error)];

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
					resources = [.. compiler.GetResources(context)];

					if (compiler.IsLocalizationSupported() && resources.Length == 0)
					{
						AnsiConsole.MarkupLine($"[yellow]Warning: no localization support for language '{context.Language.EscapeMarkup()}'.[/]");
						compilation = null;
						context.IsFatalFailure = true;
					}
					else
					{
						AnsiConsole.MarkupLine($">> [blue]Try #{context.Try}[/] Compilation [green]succeed[/] for [blue]{context.Branch.EscapeMarkup()}[/] branch.");
					}
				}
			});

		context.Try++;
		return new(compilation, archive, errors, resources);
	}

	private static async Task<ZipArchive?> GetSnapshotAsync(CompilationContext context, string branch)
	{
		var status = $"Downloading repository snapshot ({branch} branch)...";
		ZipArchive? result = null;

		try
		{
			await AnsiConsole
				.Status()
				.StartAsync(status, async statusContext =>
				{

					var handler = new HttpClientHandler
					{
						ServerCertificateCustomValidationCallback = (request, certificate, chain, errors) =>
						{
							if (errors == SslPolicyErrors.None)
								return true;

							context.IsFatalFailure = true;
							statusContext.Status = "!";
							statusContext.Refresh();

#if DEBUG
							AnsiConsole.MarkupLine($"[grey][[{errors}]] We found SSL issues.[/]");
							foreach (var chainStatus in chain?.ChainStatus ?? [])
								AnsiConsole.MarkupLine($"[grey][[{chainStatus.Status}]] {chainStatus.StatusInformation.EscapeMarkup()}[/]");
#endif

							AnsiConsole.MarkupLine($@"[yellow]Warning: We have detected a problem while retrieving the source code from {request.RequestUri?.ToString().EscapeMarkup()}[/]");
							AnsiConsole.MarkupLine(@"[yellow]Typically this is a user-side problem when something interferes with HTTPS/SSL: security or malicious software, antivirus, proxies, VPN, DNS, etc.[/]");

							if (certificate?.Subject == null)
								return true;

							AnsiConsole.MarkupLine(@$"[yellow]We got the following certificate [[{certificate.Subject.EscapeMarkup()}]] while expecting something from Github.[/]");
							AnsiConsole.MarkupLine(@"[yellow]Please try to temporarily disable any software preventing this installer from working properly.[/]");

							return true;
						}
					};

					using var client = new HttpClient(handler);
					var buffer = await client.GetByteArrayAsync(new Uri($"https://github.com/sailro/EscapeFromTarkov-Trainer/archive/refs/heads/{branch}.zip"));
					var stream = new MemoryStream(buffer);
					result = new ZipArchive(stream, ZipArchiveMode.Read);
				});
		}
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine(ex is HttpRequestException { StatusCode: HttpStatusCode.NotFound } ? $">> [blue]Try #{context.Try}[/] [yellow]Branch {branch.EscapeMarkup()} not found.[/]" : $"[red]Error: {ex.Message.EscapeMarkup()}[/]");
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

				if (nlogNode is not { Name: "nlog" } || targetsNode is not { Name: "targets" })
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
		return CreateDll(installation, filename, s =>
		{
			creator(s);
			return null;
		}, overwrite);
	}

	private static bool CreateDll(Installation installation, string filename, Func<string, EmitResult?> creator, bool overwrite = true)
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

			var result = creator(dllPath);
			if (result != null)
			{
				var errors = result
					.Diagnostics
					.Where(d => d.Severity == DiagnosticSeverity.Error)
					.ToArray();

#if DEBUG
				foreach (var error in errors)
					AnsiConsole.MarkupLine($"[grey]>> {error.Id} [[{error.Location.SourceTree?.FilePath.EscapeMarkup()}]]: {error.GetMessage().EscapeMarkup()}.[/]");
#endif

				if (!result.Success)
					throw new Exception(errors.FirstOrDefault()?.GetMessage() ?? "Unknown error while emitting assembly");
			}

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
