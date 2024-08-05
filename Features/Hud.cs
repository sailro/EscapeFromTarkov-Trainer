using EFT.InventoryLogic;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using EFT.Trainer.UI;
using UnityEngine;
using System.Text;
using EFT.Trainer.Properties;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class Hud : ToggleFeature
{
	public override string Name => Strings.FeatureHudName;
	public override string Description => Strings.FeatureHudDescription;

	[ConfigurationProperty]
	public Color Color { get; set; } = Color.white;

	[ConfigurationProperty]
	public bool ShowCompass { get; set; } = true;
		
	private static readonly string[] _directions = [
		Strings.DirectionNorth,
		Strings.DirectionNorthEast,
		Strings.DirectionEast,
		Strings.DirectionSouthEast,
		Strings.DirectionSouth,
		Strings.DirectionSouthWest,
		Strings.DirectionWest,
		Strings.DirectionNorthWest,
		Strings.DirectionNorth
	];

	[ConfigurationProperty]
	public bool ShowCoordinates { get; set; } = false;

	private readonly StringBuilder _sb = new();
	protected override void OnGUIWhenEnabled()
	{
		var player = GameState.Current?.LocalPlayer;
		if (!player.IsValid())
			return;

		var camera = GameState.Current?.Camera;
		if (camera == null)
			return;

		if (player.HandsController == null || player.HandsController.Item is not Weapon weapon)
			return;

		var mag = weapon.GetCurrentMagazine();
		if (mag == null)
			return;

		_sb.Clear();

		if (ShowCompass)
		{
			var forward = camera.transform.forward;
			forward.y = 0;

			var heading = Quaternion.LookRotation(forward).eulerAngles.y;
			_sb.Append(_directions[(int)Mathf.Round(heading % 360 / 45)]);
			_sb.Append(Strings.FeatureHudSeparator);
		}

		_sb.Append(string.Format(Strings.FeatureHudWeaponFormat, mag.Count, weapon.ChamberAmmoCount, mag.MaxCount, weapon.SelectedFireMode));

		if (ShowCoordinates)
		{
			_sb.Append(Strings.FeatureHudSeparator);
			var position = player.Transform.position;
			_sb.Append(string.Format(Strings.FeatureHudCoordinatesFormat, Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z)));
		}

		Render.DrawString(new Vector2(512, Screen.height - 16f), _sb.ToString(), Color);
	}
}
