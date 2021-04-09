using System.Diagnostics.CodeAnalysis;
using EFT.Interactive;

#nullable enable

namespace EFT.Trainer.Extensions
{
	public static class DoorExtensions
	{
		public static bool IsValid([NotNullWhen(true)] this Door? door)
		{
			return door != null;
		}
	}
}
