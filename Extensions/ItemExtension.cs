using System.Diagnostics.CodeAnalysis;
using EFT.InventoryLogic;

#nullable enable

namespace EFT.Trainer.Extensions
{
	public static class ItemExtension
	{
		public static bool IsValid([NotNullWhen(true)] this Item? item)
		{
			return item?.Template != null;
		}

		public static bool IsFiltered(this Item item)
		{
			if (string.IsNullOrEmpty(item.TemplateId))
				return true;

			return item.TemplateId switch
			{
				KnownTemplateIds.Dollars or KnownTemplateIds.Euros or KnownTemplateIds.Roubles => false,
				_ => item.Weight <= 0f,// easy way to remove special items like "Pockets" or "Default Inventory"
			};
		}
	}
}
