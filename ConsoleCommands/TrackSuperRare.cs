using JetBrains.Annotations;
using JsonType;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

[UsedImplicitly]
internal class TrackSuperRare : BaseTrackCommand
{
	public override string Name => "tracksr";
	protected override ELootRarity? Rarity => ELootRarity.Superrare;
}
