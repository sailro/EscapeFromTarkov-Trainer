using EFT.Trainer.Properties;
using JetBrains.Annotations;
using JsonType;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

[UsedImplicitly]
internal class TrackSuperRare : BaseTrackCommand
{
	public override string Name => Strings.CommandTrackSuperRare;
	protected override ELootRarity? Rarity => ELootRarity.Superrare;
}
