using JetBrains.Annotations;
using JsonType;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

[UsedImplicitly]
internal class List : BaseListCommand
{
	public override string Name => "list";
}
