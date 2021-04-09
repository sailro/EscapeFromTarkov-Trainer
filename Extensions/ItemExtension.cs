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
	}
}
