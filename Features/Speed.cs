using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	[UsedImplicitly]
	internal class Speed : HoldFeature
	{
		public override string Name => "speed";

		public override KeyCode Key { get; set; } = KeyCode.None;

		[ConfigurationProperty]
		public float Intensity { get; set; } = 2.0f;

		protected override void UpdateWhenHold()
		{
			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return;

			var camera = GameState.Current?.Camera;
			if (camera == null)
				return;

			player.Transform.position += Intensity * Time.deltaTime * camera.transform.forward;
		}
	}
}
