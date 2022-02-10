using EFT.InventoryLogic;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using EFT.Trainer.UI;
using UnityEngine;
using System;
using System.Text;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.Features
{
	[UsedImplicitly]
	internal class Hud : ToggleFeature
	{
		public override string Name => "hud";

		[ConfigurationProperty]
		public Color Color { get; set; } = Color.white;

		[ConfigurationProperty]
		public bool ShowCompass { get; set; } = true;
		
		private static readonly string[] _directions = { "N", "NE", "E", "SE", "S", "SW", "W", "NW", "N" };

		[ConfigurationProperty]
		public bool ShowCoordinates { get; set; } = false;

		private readonly StringBuilder _sb = new();
		protected override void OnGUIWhenEnabled()
		{
			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return;

			if (player.HandsController == null || player.HandsController.Item is not Weapon weapon)
				return;

			var mag = weapon.GetCurrentMagazine();
			if (mag == null)
				return;

			_sb.Clear();
			const string separator = " - ";

			if (ShowCompass)
			{
				var forward = player.Transform.forward;
				forward.y = 0;

				var heading = Quaternion.LookRotation(forward).eulerAngles.y;
				_sb.Append(_directions[(int)Math.Round((double)heading % 360 / 45)]);
				_sb.Append(separator);
			}

			_sb.Append($"{mag.Count}+{weapon.ChamberAmmoCount}/{mag.MaxCount} [{weapon.SelectedFireMode}]");

			if (ShowCoordinates)
			{
				_sb.Append(separator);
				var position = player.Transform.position;
				_sb.Append($"({Mathf.RoundToInt(position.x)},{Mathf.RoundToInt(position.z)})");
			}

			Render.DrawString(new Vector2(512, Screen.height - 16f), _sb.ToString(), Color);
		}
	}
}
