using JetBrains.Annotations;
using JsonType;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

[UsedImplicitly]
internal class ListSuperRare : BaseListCommand
{
	public override string Name => "listsr";
	protected override ELootRarity? Rarity => ELootRarity.Superrare;
}
