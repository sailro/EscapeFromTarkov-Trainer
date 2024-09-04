using System.Diagnostics.CodeAnalysis;
using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;

#nullable enable

namespace EFT.Trainer.Extensions;

public static class ItemExtensions
{
	public static bool IsValid([NotNullWhen(true)] this Item? item)
	{
		return item?.Template != null;
	}

	public static bool IsFiltered(this Item item)
	{
		if (string.IsNullOrEmpty(item.TemplateId))
			return true;

		if (ItemViewFactory.IsSecureContainer(item))
			return true;

		if (item.CurrentAddress?.Container?.ParentItem?.TemplateId == (MongoID)KnownTemplateIds.BossContainer)
			return true;

		if (item.TemplateId == (MongoID)KnownTemplateIds.DefaultInventory || item.TemplateId == (MongoID)KnownTemplateIds.Pockets)
			return true;

		// KnownTemplateIds.Dollars or KnownTemplateIds.Euros or KnownTemplateIds.Roubles => false,
		// Incompatible with extra mods like AllInOne, setting item weight to zero
		//_ => item.Weight <= 0f,// easy way to remove special items like "Pockets" or "Default Inventory"

		return false;
	}
}
