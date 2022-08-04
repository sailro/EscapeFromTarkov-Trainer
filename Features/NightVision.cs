﻿using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	[UsedImplicitly]
	internal class NightVision : ToggleFeature
	{
		public override string Name => "night";

		public override bool Enabled { get; set; } = false;

		protected override void Update()
		{
			base.Update();
		}

		protected override void UpdateWhenEnabled()
		{
			base.UpdateWhenEnabled();
			SwitchNV();
		}

		protected override void OnDisableFeature()
		{
			base.OnDisableFeature();
			SwitchNV();
		}

		protected void SwitchNV()
		{
			if (GameState.Current?.LocalPlayer is HideoutPlayer)
				return;

			var camera = GameState.Current?.Camera;
			if (camera == null)
				return;

			var component = camera.GetComponent<BSG.CameraEffects.NightVision>();
			if (component == null || component.On == Enabled)
				return;

			component.StartSwitch(Enabled);

			if (!Enabled)
				return;

			// component.DiffuseIntensity = 0f; removed with 0.12.12.19078
			component.Intensity = 0f;
			component.NoiseIntensity = 0f;

			component.TextureMask.Color = new Color(0f, 0f, 0f, 0f);
			component.TextureMask.Stretch = false;
			component.TextureMask.Size = 0f;
		}
	}
}
