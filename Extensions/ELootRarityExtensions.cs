using JsonType;

#nullable enable

namespace EFT.Trainer.Extensions
{
	public static class ELootRarityExtensions
	{
		public static string Color(this ELootRarity rarity)
		{
			return rarity switch
			{
				ELootRarity.Superrare => ELootRarity.Superrare.ToString().Red(),
				ELootRarity.Rare => ELootRarity.Rare.ToString().Yellow(),
				ELootRarity.Common => ELootRarity.Common.ToString().Green(),
				ELootRarity.Not_exist => ELootRarity.Not_exist.ToString(),
				_ => string.Empty
			};
		}
	}
}
