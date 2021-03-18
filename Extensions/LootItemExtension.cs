using EFT.Interactive;

namespace EFT.Trainer.Extensions
{
	public static class LootItemExtension
	{
		public static bool IsValid(this LootItem lootItem)
		{
			return lootItem != null
			       && lootItem.Item?.Template != null;
		}
	}
}
