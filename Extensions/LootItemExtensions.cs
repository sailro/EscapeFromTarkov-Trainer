using System.Diagnostics.CodeAnalysis;
using EFT.Interactive;

#nullable enable

namespace EFT.Trainer.Extensions
{
	public static class LootItemExtensions
	{
		public static bool IsValid([NotNullWhen(true)] this LootItem? lootItem)
		{
			return lootItem != null
			       && lootItem.Item.IsValid();
		}
	}
}
