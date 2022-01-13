using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Spectre.Console;
using Spectre.Console.Cli;

#nullable enable

namespace Installer
{
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
		}

		public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
		{
			try
			{
				AnsiConsole.MarkupLine("-=[[ [cyan]EscapeFromTarkov-Trainer Universal Installer[/] - [blue]https://github.com/sailro [/]]]=-");
				AnsiConsole.WriteLine();

				var installation = Installation.GetTargetInstallation(settings.Path, "Please select where to install the trainer");
				if (installation == null)
					return (int)ExitCode.NoInstallationFound;

				AnsiConsole.MarkupLine($"Target [green]EscapeFromTarkov ({installation.Version})[/] in [blue]{installation.Location.EscapeMarkup()}[/].");

				// Try first to compile against master
				var @try = 0;
				var (compilation, archive, errors) = await GetCompilationAsync(++@try, installation, "master");
				var files = errors
					.Select(d => d.Location.SourceTree?.FilePath)
					.Where(s => s is not null)
					.Distinct()
					.ToArray();

				if (compilation == null)
				{
					// Failure, so try with a dedicated branch if exists
					var branch = settings.Branch ?? installation.Version.ToString();
					if (!branch.StartsWith("dev-"))
						branch = "dev-" + branch;

					(compilation, archive, _) = await GetCompilationAsync(++@try, installation, branch);
				}

				if (compilation == null && files.Any() && files.All(f => f!.StartsWith("Features")))
				{
					// Failure, retry by removing faulting features if possible
					AnsiConsole.MarkupLine($"[yellow]Trying to disable faulting feature(s): [red]{string.Join(", ", files.Select(Path.GetFileNameWithoutExtension))}[/].[/]");
					(compilation, archive, errors) = await GetCompilationAsync(++@try, installation, "master", files!);

					if (!errors.Any())
						AnsiConsole.MarkupLine("[yellow]We found a fallback! But please file an issue here : https://github.com/sailro/EscapeFromTarkov-Trainer/issues [/]");
				}

				if (compilation == null)
				{
					// Failure
					AnsiConsole.MarkupLine($"[red]Unable to compile trainer for version {installation.Version}. Please file an issue here : https://github.com/sailro/EscapeFromTarkov-Trainer/issues [/]");
					return (int)ExitCode.CompilationFailed;
				}

				if (installation.UsingSptAki)
				{
					AnsiConsole.MarkupLine("[yellow]SPT-AKI detected. Please make sure you have run the game at least once before installing the trainer.[/]");
					AnsiConsole.MarkupLine("[yellow]SPT-AKI is patching binaries during the first run, and we [underline]need[/] to compile against those patched binaries.[/]");
					AnsiConsole.MarkupLine("[yellow]If you install this trainer on stock binaries, the game will freeze at the startup screen.[/]");

					if (!AnsiConsole.Confirm("Continue installation (yes I have run the game at least once) ?"))
						return (int)ExitCode.Canceled;
				}

				if (!CreateDll(installation, compilation))
					return (int)ExitCode.CreateDllFailed;

				if (!CreateOutline(installation, archive!))
					return (int)ExitCode.CreateOutlineFailed;

				CreateOrPatchConfiguration(installation);
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Error: {ex.Message.EscapeMarkup()}. Please file an issue here : https://github.com/sailro/EscapeFromTarkov-Trainer/issues [/]");
				return (int)ExitCode.Failure;
			}

			return (int)ExitCode.Success;
		}

		private static async Task<(CSharpCompilation?, ZipArchive?, Diagnostic[])> GetCompilationAsync(int @try, Installation installation, string branch, params string[] exclude)
		{
			var errors = Array.Empty<Diagnostic>();

			var archive = await GetSnapshotAsync(@try, branch);
			if (archive == null)
				return (null, null, errors);

			CSharpCompilation? compilation = null;
			AnsiConsole
				.Status()
				.Start("Compiling trainer", _ =>
				{
					var compiler = new Compiler(archive, installation, exclude);
					compilation = compiler.Compile();
					errors = compilation
						.GetDiagnostics()
						.Where(d => d.Severity == DiagnosticSeverity.Error)
						.ToArray();

#if DEBUG
					foreach (var error in errors)
						AnsiConsole.MarkupLine($">> {error.Id} [[{error.Location.SourceTree?.FilePath}]]: {error.GetMessage()}");
#endif

					if (errors.Any())
					{
						AnsiConsole.MarkupLine($">> [blue]Try #{@try}[/] [yellow]Compilation failed for {branch.EscapeMarkup()} branch.[/]");
						compilation = null;
					}
					else
					{
						AnsiConsole.MarkupLine($">> [blue]Try #{@try}[/] Compilation [green]succeed[/] for [blue]{branch.EscapeMarkup()}[/] branch.");
					}
				});

			return (compilation, archive, errors);
		}

		private static async Task<ZipArchive?> GetSnapshotAsync(int @try, string branch)
		{
			var status = $"Downloading repository snapshot ({branch} branch)...";
			ZipArchive? result = null;

			try
			{
				await AnsiConsole
					.Status()
					.StartAsync(status, async ctx =>
					{
						using var client = new WebClient();
						client.DownloadProgressChanged += (_, eventArgs) =>
						{
							ctx.Status($"{status}{eventArgs.BytesReceived / 1024}K");
						};

						var buffer = await client.DownloadDataTaskAsync(new Uri($"https://github.com/sailro/EscapeFromTarkov-Trainer/archive/refs/heads/{branch}.zip"));
						var stream = new MemoryStream(buffer);
						result = new ZipArchive(stream, ZipArchiveMode.Read);
					});
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine(ex is WebException {Response: HttpWebResponse {StatusCode: HttpStatusCode.NotFound}} ? $">> [blue]Try #{@try}[/] [yellow]Branch {branch.EscapeMarkup()} not found.[/]" : $"[red]Error: {ex.Message.EscapeMarkup()}[/]");
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

					if (targetsNode.ChildNodes.Cast<XmlNode>().Any(targetNode => targetNode.Attributes?["name"].Value == targetName && targetNode.Attributes["xsi:type"].Value == targetName))
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
				input.CopyToAsync(output);

				AnsiConsole.MarkupLine($"Created [green]{Path.GetFileName(outlinePath).EscapeMarkup()}[/] in [blue]{Path.GetDirectoryName(outlinePath).EscapeMarkup()}[/].");
				return true;
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Unable to create {outlinePath.EscapeMarkup()}: {ex.Message.EscapeMarkup()}.[/]");
				return false;
			}
		}

		private static bool CreateDll(Installation installation, CSharpCompilation compilation)
		{
			var dllPath = Path.Combine(installation.Managed, "NLog.EFT.Trainer.dll");
			try
			{
				compilation.Emit(dllPath);
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
}
