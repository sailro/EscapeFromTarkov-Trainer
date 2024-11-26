using System;
using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT.InventoryLogic;

#nullable enable

namespace EFT.Trainer.Features;

internal class TemplateHelper
{
	// We cannot properly search by "partial" mongoId, even if we have implicit conversion to string
	// So keep our own cache of templates
	private static readonly Dictionary<string, ItemTemplate> _templates = [];

	private static void UpdateTemplates()
	{
		if (!Singleton<ItemFactoryClass>.Instantiated)
			return;

		var mongoTemplates = Singleton<ItemFactoryClass>
			.Instance
			.ItemTemplates;

		if (_templates.Count == mongoTemplates.Count)
			return;

		foreach (var kv in mongoTemplates)
		{
			_templates.Add(kv.Key.ToString(), kv.Value);
		}
	}

	internal static ItemTemplate[] FindTemplates(string searchShortNameOrTemplateId)
	{
		UpdateTemplates();

		// Match by TemplateId
		if (_templates.TryGetValue(searchShortNameOrTemplateId, out var template))
		{
			return [template];
		}

		// Match by short name(s)
		return _templates
			.Values
			.Where(t => t.ShortNameLocalizationKey.Localized().IndexOf(searchShortNameOrTemplateId, StringComparison.OrdinalIgnoreCase) >= 0
						|| t.NameLocalizationKey.Localized().IndexOf(searchShortNameOrTemplateId, StringComparison.OrdinalIgnoreCase) >= 0)
			.ToArray();
	}
}
