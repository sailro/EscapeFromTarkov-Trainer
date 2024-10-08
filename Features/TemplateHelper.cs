using System;
using System.Linq;
using Comfort.Common;
using EFT.InventoryLogic;

#nullable enable

namespace EFT.Trainer.Features;

internal class TemplateHelper
{
	internal static ItemTemplate[] FindTemplates(string searchShortNameOrTemplateId)
	{
		if (!Singleton<ItemFactoryClass>.Instantiated)
			return [];

		var templates = Singleton<ItemFactoryClass>
			.Instance
			.ItemTemplates;

		// Match by TemplateId
		if (templates.TryGetValue(searchShortNameOrTemplateId, out var template))
			return [template];

		// Match by short name(s)
		return templates
			.Values
			.Where(t => t.ShortNameLocalizationKey.Localized().IndexOf(searchShortNameOrTemplateId, StringComparison.OrdinalIgnoreCase) >= 0
						|| t.NameLocalizationKey.Localized().IndexOf(searchShortNameOrTemplateId, StringComparison.OrdinalIgnoreCase) >= 0)
			.ToArray();
	}
}
