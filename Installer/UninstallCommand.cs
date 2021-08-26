using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Spectre.Console;
using Spectre.Console.Cli;

#nullable enable

namespace Installer
{
	internal sealed class UninstallCommand : Command<UninstallCommand.Settings>
	{
		internal class Settings : CommandSettings
		{
			[Description("Path to EFT.")]
			[CommandArgument(0, "[path]")]
			public string? Path { get; set; }
		}

		public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
		{
			try
			{
				AnsiConsole.MarkupLine("[cyan]-=[[ EscapeFromTarkov-Trainer Universal Installer ]]=-[/]");
				AnsiConsole.WriteLine();

				var installation = Installation.GetTargetInstallation(settings.Path, "Please select from where to uninstall the trainer");
				if (installation == null)
					return (int)ExitCode.NoInstallationFound;

				AnsiConsole.MarkupLine($"Target [green]EscapeFromTarkov ({installation.Version})[/] in [blue]{installation.Location}[/].");

				if (!RemoveFile(Path.Combine(installation.Managed, "NLog.EFT.Trainer.dll")))
					return (int)ExitCode.RemoveDllFailed;

				if (!RemoveFile(Path.Combine(installation.Data, "outline")))
					return (int)ExitCode.RemoveOutlineFailed;

				RemoveOrPatchConfiguration(installation);
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Error: {ex.Message}. Please file an issue here : https://github.com/sailro/EscapeFromTarkov-Trainer/issues [/]");
				return (int)ExitCode.Failure;
			}

			return (int)ExitCode.Success;
		}

		private static bool RemoveFile(string filename)
		{
			try
			{
				if (!File.Exists(filename))
				{
					AnsiConsole.MarkupLine($"No [green]{Path.GetFileName(filename)}[/] in [blue]{Path.GetDirectoryName(filename)}[/].");
				}
				else
				{
					File.Delete(filename);
					AnsiConsole.MarkupLine($"Removed [green]{Path.GetFileName(filename)}[/] in [blue]{Path.GetDirectoryName(filename)}[/].");
				}

				return true;
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Unable to remove {filename}: {ex.Message} [/]");
				return false;
			}
		}

		private static void RemoveOrPatchConfiguration(Installation installation)
		{
			const string targetName = "EFTTarget";
			var configPath = Path.Combine(installation.Managed, "NLog.dll.nlog");
			try
			{
				if (!File.Exists(configPath))
				{
					// Only for display
					RemoveFile(configPath);
					return;
				}

				var doc = new XmlDocument();
				doc.Load(configPath);

				var nlogNode = doc.DocumentElement;
				var targetsNode = nlogNode?.FirstChild;

				if (nlogNode is not {Name: "nlog"} || targetsNode is not {Name: "targets"})
				{
					AnsiConsole.MarkupLine($"[red]Unable to unpatch {configPath}, unexpected xml structure.[/]");
					return;
				}

				var removeNodes = targetsNode
					.ChildNodes
					.Cast<XmlNode>()
					.Where(targetNode => targetNode.Attributes?["name"].Value == targetName && targetNode.Attributes["xsi:type"].Value == targetName)
					.ToList();

				if (removeNodes.Count == 0)
				{
					AnsiConsole.MarkupLine($"Not patched [green]{Path.GetFileName(configPath)}[/] in [blue]{Path.GetDirectoryName(configPath)}[/].");
					return;
				}

				foreach (var target in removeNodes)
					targetsNode.RemoveChild(target);

				if (targetsNode.HasChildNodes)
				{
					var builder = new StringBuilder();
					using var writer = new UTF8StringWriter(builder);
					doc.Save(writer);
					builder.Replace(" xmlns=\"\"", string.Empty);
					File.WriteAllText(configPath, builder.ToString());

					AnsiConsole.MarkupLine($"Unpatched [green]{Path.GetFileName(configPath)}[/] in [blue]{Path.GetDirectoryName(configPath)}[/].");
				}
				else
				{
					RemoveFile(configPath);
				}
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Unable to unpatch or remove {configPath}: {ex.Message}.[/]");
			}
		}
	}
}
