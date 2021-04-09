using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace EFT.Trainer.Extensions
{
	public static class ThrowableExtensions
	{
		public static bool IsValid([NotNullWhen(true)] this Throwable? throwable)
		{
			return throwable != null
			       && throwable.transform != null;
		}
	}
}
