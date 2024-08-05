using EFT.Trainer.Properties;
using JetBrains.Annotations;
using JsonType;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

[UsedImplicitly]
internal class ListRare : BaseListCommand
{
	public override string Name => Strings.CommandListRare;
	protected override ELootRarity? Rarity => ELootRarity.Rare;
}
