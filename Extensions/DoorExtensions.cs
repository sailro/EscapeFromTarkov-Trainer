using EFT.Interactive;

namespace EFT.Trainer.Extensions
{
	public static class DoorExtensions
	{
		public static bool IsValid(this Door door)
		{
			return door != null;
		}
	}
}
