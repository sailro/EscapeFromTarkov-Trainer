using System.Linq;
using System.Text.RegularExpressions;
using Comfort.Common;
using EFT.Trainer.Extensions;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

[UsedImplicitly]
internal class Template : BaseTemplateCommand
{
	public override string Name => "template";

	public override void Execute(Match match)
	{
		var matchGroup = match.Groups[ValueGroup];
		if (matchGroup is not {Success: true})
			return;

		if (!Singleton<ItemFactory>.Instantiated)
			return;

		var search = matchGroup.Value;

		var templates = FindTemplates(search).ToArray();
		
		foreach (var template in templates)
			AddConsoleLog($"{template._id}: {template.ShortNameLocalizationKey.Localized().Green()} [{template.NameLocalizationKey.Localized()}]");

		AddConsoleLog("------");
		AddConsoleLog($"found {templates.Length.ToString().Cyan()} template(s)");
	}
}
