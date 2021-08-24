using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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

		public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] Settings settings)
		{
			try
			{
				AnsiConsole.MarkupLine("[cyan]-=[[ EscapeFromTarkov-Trainer Universal Installer ]]=-[/]");
				AnsiConsole.WriteLine();

				var installation = Installation.GetTargetInstallation(settings.Path, "Please select where to install the trainer");
				if (installation == null)
					return (int)ExitCode.NoInstallationFound;

				AnsiConsole.MarkupLine($"Target [green]EscapeFromTarkov ({installation.Version})[/] installation is [blue]{installation.Location}[/].");

				var (compilation, archive) = await GetCompilationAsync(installation, "master");
				if (compilation == null)
				{
					var branch = settings.Branch ?? installation.Version.ToString();
					if (!branch.StartsWith("dev-"))
						branch = "dev-" + branch;

					(compilation, archive) = await GetCompilationAsync(installation, branch);
				}

				if (compilation == null)
				{
					AnsiConsole.MarkupLine($"[red]Unable to compile trainer for version {installation.Version}. Please file an issue here : https://github.com/sailro/EscapeFromTarkov-Trainer/issues [/]");
					return (int)ExitCode.CompilationFailed;
				}

				if (installation.SptAkiVersion != null)
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
				AnsiConsole.MarkupLine($"[red]Error: {ex.Message}. Please file an issue here : https://github.com/sailro/EscapeFromTarkov-Trainer/issues [/]");
				return (int)ExitCode.Failure;
			}

			return (int)ExitCode.Success;
		}

		private static async Task<(CSharpCompilation?, ZipArchive?)> GetCompilationAsync(Installation installation, string branch)
		{
			var archive = await GetSnapshotAsync(branch);
			if (archive == null)
				return (null, null);

			CSharpCompilation? compilation = null;
			AnsiConsole
				.Status()
				.Start("Compiling trainer", _ =>
				{
					var compiler = new Compiler(archive, installation);
					compilation = compiler.Compile();
					var errors = compilation
						.GetDiagnostics()
						.Where(d => d.Severity == DiagnosticSeverity.Error)
						.ToList();

					if (errors.Any())
					{
						AnsiConsole.MarkupLine($"[yellow]Compilation failed for {branch} branch.[/]");
						compilation = null;
					}
					else
					{
						AnsiConsole.MarkupLine($"Compilation [green]succeded[/] for [blue]{branch}[/] branch.");
					}
				});

			return (compilation, archive);
		}

		private static async Task<ZipArchive?> GetSnapshotAsync(string branch)
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
				AnsiConsole.MarkupLine(ex is WebException {Response: HttpWebResponse {StatusCode: HttpStatusCode.NotFound}} ? $"[yellow]Branch {branch} not found.[/]" : $"[red]Error: {ex.Message}[/]");
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
						AnsiConsole.MarkupLine($"[red]Unable to patch {configPath}, unexpected xml structure.[/]");
						return;
					}

					if (targetsNode.ChildNodes.Cast<XmlNode>().Any(targetNode => targetNode.Attributes?["name"].Value == targetName && targetNode.Attributes["xsi:type"].Value == targetName))
					{
						AnsiConsole.MarkupLine($"Already patched [green]{Path.GetFileName(configPath)}[/] in [blue]{Path.GetDirectoryName(configPath)}[/].");
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

					AnsiConsole.MarkupLine($"Patched [green]{Path.GetFileName(configPath)}[/] in [blue]{Path.GetDirectoryName(configPath)}[/].");
					return;
				}

				var content = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<nlog xmlns=""http://www.nlog-project.org/schemas/NLog.xsd"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <targets>
    <target name=""{targetName}"" xsi:type=""{targetName}"" />
  </targets>
</nlog>";
				File.WriteAllText(configPath, content);
				AnsiConsole.MarkupLine($"Created [green]{Path.GetFileName(configPath)}[/] in [blue]{Path.GetDirectoryName(configPath)}[/].");
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Unable to patch or create {configPath}: {ex.Message}.[/]");
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

				AnsiConsole.MarkupLine($"Created [green]{Path.GetFileName(outlinePath)}[/] in [blue]{Path.GetDirectoryName(outlinePath)}[/].");
				return true;
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Unable to create {outlinePath}: {ex.Message}.[/]");
				return false;
			}
		}

		private static bool CreateDll(Installation installation, CSharpCompilation compilation)
		{
			var dllPath = Path.Combine(installation.Managed, "NLog.EFT.Trainer.dll");
			try
			{
				compilation.Emit(dllPath);
				AnsiConsole.MarkupLine($"Created [green]{Path.GetFileName(dllPath)}[/] in [blue]{Path.GetDirectoryName(dllPath)}[/].");
				return true;
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Unable to create {dllPath}: {ex.Message} [/]");
				return false;
			}
		}
	}
}
