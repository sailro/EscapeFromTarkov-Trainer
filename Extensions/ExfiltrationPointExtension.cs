using EFT.Interactive;

namespace EFT.Trainer.Extensions
{
	public static class ExfiltrationPointExtension
	{
		public static bool IsValid(this ExfiltrationPoint point)
		{
			return point != null 
			       && point.Settings?.Name != null
			       && point.transform != null;
		}
	}
}
