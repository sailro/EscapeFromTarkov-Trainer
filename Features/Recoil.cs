using EFT.Trainer.Extensions;
using UnityEngine;

namespace EFT.Trainer.Features
{
	public class Recoil : MonoBehaviour, IEnableable
	{
		public bool Enabled { get; set; } = false;

		public void Update()
		{
			if (!Enabled)
				return;

			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return;

			player!.ProceduralWeaponAnimation.Shootingg.Intensity = 0f;
		}
	}
}
