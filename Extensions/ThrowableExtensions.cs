namespace EFT.Trainer.Extensions
{
	public static class ThrowableExtensions
	{
		public static bool IsValid(this Throwable throwable)
		{
			return throwable != null
			       && throwable.transform != null;
		}
	}
}
