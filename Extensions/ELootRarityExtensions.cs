using JsonType;

#nullable enable

namespace EFT.Trainer.Extensions;

public static class ELootRarityExtensions
{
	public static string Color(this ELootRarity rarity)
	{
		return rarity switch
		{
			ELootRarity.Superrare => nameof(ELootRarity.Superrare).Red(),
			ELootRarity.Rare => nameof(ELootRarity.Rare).Yellow(),
			ELootRarity.Common => nameof(ELootRarity.Common).Green(),
			ELootRarity.Not_exist => nameof(ELootRarity.Not_exist),
			_ => string.Empty
		};
	}
}
