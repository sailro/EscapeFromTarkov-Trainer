using Comfort.Common;
using EFT.Ballistics;

namespace EFT.Trainer.Features
{
	public class WallShoot : CachableMonoBehaviour<BallisticCollider[]>
	{
		public override float CacheTimeInSec { get; set; } = 2f;

		public override BallisticCollider[] RefreshData()
		{
			return FindObjectsOfType<BallisticCollider>();
		}

		public override void ProcessData(BallisticCollider[] data)
		{
			var world = Singleton<GameWorld>.Instance;
			if (world == null)
				return;

			var sbc = world.SharedBallisticsCalculator;
			if (sbc == null)
				return;

			for (int shotIndex = 0; shotIndex < sbc.ActiveShotsCount; shotIndex++)
			{
				var shot = sbc.GetActiveShot(shotIndex);
				if (shot.IsShotFinished)
					continue;

				shot.IsForwardHit = false;
				shot.PenetrationPower = 100f;
			}

			foreach(var bc in data)
			{
				bc.PenetrationChance = 1.0f;
				bc.PenetrationLevel = 0.0f;
				bc.RicochetChance = 0.0f;
				bc.FragmentationChance = 0.0f;
				bc.TrajectoryDeviationChance = 0.0f;
				bc.TrajectoryDeviation = 0.0f;
			}
		}
	}
}
