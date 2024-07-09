using EFT.Trainer.Properties;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

[UsedImplicitly]
internal class List : BaseListCommand
{
	public override string Name => Strings.CommandList;
}
