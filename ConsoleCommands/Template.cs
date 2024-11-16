using System.Text.RegularExpressions;
using EFT.Trainer.Extensions;
using EFT.Trainer.Features;
using EFT.Trainer.Properties;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

[UsedImplicitly]
internal class Template : BaseTemplateCommand
{
	public override string Name => Strings.CommandTemplate;

	public override void Execute(Match match)
	{
		var matchGroup = match.Groups[ValueGroup];
		if (matchGroup is not { Success: true })
			return;

		var search = matchGroup.Value;

		var templates = TemplateHelper.FindTemplates(search);

		foreach (var template in templates)
			AddConsoleLog(string.Format(Strings.CommandTemplateEnumerateFormat, template._id, template.ShortNameLocalizationKey.Localized().Green(), template.NameLocalizationKey.Localized()));

		AddConsoleLog(Strings.TextSeparator);
		AddConsoleLog(string.Format(Strings.CommandTemplateSuccessFormat, templates.Length.ToString().Cyan()));
	}
}
