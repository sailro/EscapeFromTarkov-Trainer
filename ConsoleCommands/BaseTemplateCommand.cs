using System;
using System.Collections.Generic;
using System.Linq;
#nullable enable

namespace EFT.Trainer.ConsoleCommands;

internal abstract class BaseTemplateCommand : ConsoleCommandWithArgument
{
	public override string Pattern => RequiredArgumentPattern;
}
