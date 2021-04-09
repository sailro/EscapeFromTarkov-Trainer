using NLog.Targets;

#nullable enable

namespace EFT.Trainer
{
	[Target(nameof(EFTTarget))]
	public sealed class EFTTarget : TargetWithLayout
	{
		public EFTTarget()
		{
			Loader.Load();
		}
	}
}
