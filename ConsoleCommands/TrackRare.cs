using JetBrains.Annotations;
using JsonType;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

[UsedImplicitly]
internal class TrackRare : BaseTrackCommand
{
	public override string Name => "trackr";
	protected override ELootRarity? Rarity => ELootRarity.Rare;
}
