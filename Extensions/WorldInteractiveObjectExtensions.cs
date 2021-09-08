using System.Diagnostics.CodeAnalysis;
using EFT.Interactive;

#nullable enable

namespace EFT.Trainer.Extensions
{
	public static class WorldInteractiveObjectExtensions
	{
		public static bool IsValid([NotNullWhen(true)] this WorldInteractiveObject? obj)
		{
			return obj != null;
		}
	}
}
