using System.Collections.Generic;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using EFT.Trainer.UI;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features;

internal abstract class BaseMapToggleFeature : ToggleFeature
{
	[ConfigurationProperty(Order = 40)]
	public bool ShowPlayers { get; set; } = true;

	[ConfigurationProperty(Order = 40)]
	public bool ShowScavs { get; set; } = true;

	[ConfigurationProperty(Order = 40)]
	public bool ShowScavRaiders { get; set; } = true;

	[ConfigurationProperty(Order = 40)]
	public bool ShowScavAssaults { get; set; } = true;

	[ConfigurationProperty(Order = 40)]
	public bool ShowBosses { get; set; } = true;

	[ConfigurationProperty(Order = 40)]
	public bool ShowCultists { get; set; } = true;

	[ConfigurationProperty(Order = 40)]
	public bool ShowRogues { get; set; } = true;

	[ConfigurationProperty(Order = 40)]
	public bool ShowMarksmen { get; set; } = true;

	[ConfigurationProperty(Order = 90)]
	public bool ChangeTime { get; set; } = false;

	private GameObject? _mapCameraObject = null;
	private Camera? _mapCamera = null;

	protected Camera? MapCamera => _mapCamera;

	private static readonly string[] _directions = [
		Properties.Strings.DirectionNorth,
		Properties.Strings.DirectionNorthEast,
		Properties.Strings.DirectionEast,
		Properties.Strings.DirectionSouthEast,
		Properties.Strings.DirectionSouth,
		Properties.Strings.DirectionSouthWest,
		Properties.Strings.DirectionWest,
		Properties.Strings.DirectionNorthWest,
		Properties.Strings.DirectionNorth
	];

	protected void ToggleMapCameraIfNeeded(bool state)
	{
		if (_mapCamera == null)
			return;

		if (_mapCamera.enabled == state)
			return;

		_mapCamera.enabled = state;
	}

	protected void SetupMapCamera(Camera camera, float x, float y, float sizex, float sizey)
	{
		if (_mapCamera != null)
		{
			_mapCamera.pixelRect = new Rect(x, y, sizex, sizey);
			ToggleMapCameraIfNeeded(true);
			return;
		}

		// We need to setup weather for proper rendering
		Weather.ToClearWeather(ChangeTime);

		_mapCameraObject = new GameObject(GetType().FullName + nameof(_mapCameraObject), typeof(Camera), typeof(PrismEffects));
		_mapCameraObject.GetComponent<PrismEffects>().SetPrismPreset(camera.GetComponent<PrismEffects>().currentPrismPreset);
		_mapCamera = _mapCameraObject.GetComponent<Camera>();
		_mapCamera.name = GetType().FullName + nameof(_mapCamera);
		_mapCamera.pixelRect = new Rect(x, y, sizex, sizey);
		_mapCamera.allowHDR = false;
		_mapCamera.depth = -1;

		// Prevent NullReferenceException in PrismEffects 
		GameWorld.OnDispose -= UpdateWhenDisabled;
		GameWorld.OnDispose += UpdateWhenDisabled;
	}

	protected void UpdateMapCamera(Camera camera, float range)
	{
		if (_mapCameraObject == null)
			return;

		var cameraTransform = camera.transform;

		var mapTransform = _mapCameraObject.transform;
		mapTransform.eulerAngles = new Vector3(90, cameraTransform.eulerAngles.y, cameraTransform.eulerAngles.z);
		mapTransform.localPosition = new Vector3(cameraTransform.localPosition.x, range * Mathf.Tan(45), cameraTransform.localPosition.z);
	}

	protected void DrawHostiles(Camera camera, IEnumerable<Player> hostiles, float range)
	{
		var cameraPosition = camera.transform.position;


		var feature = FeatureFactory.GetFeature<Players>();
		if (feature == null)
			return;

		foreach (var enemy in hostiles)
		{
			if (!enemy.IsValid())
				continue;

			var position = enemy.Transform.position;

			if (MapCamera != null && MapCamera.enabled)
				cameraPosition.y = position.y;

			var distance = Vector3.Distance(cameraPosition, position);
			if (range > 0 && distance > range)
				continue;

			var hostileType = enemy.GetHostileType();

			switch (hostileType)
			{
				case HostileType.Scav when !ShowScavs:
				case HostileType.ScavRaider when !ShowScavRaiders:
				case HostileType.Cultist when !ShowCultists:
				case HostileType.Boss when !ShowBosses:
				case HostileType.ScavAssault when !ShowScavAssaults:
				case HostileType.Marksman when !ShowMarksmen:
				case HostileType.RogueUsec when !ShowRogues:
				case HostileType.Bear or HostileType.Usec when !ShowPlayers:
					continue;

				default:
					{
						var playerColor = feature.GetPlayerColors(hostileType);
						DrawEnemy(camera, enemy, playerColor.Color);
						break;
					}
			}
		}
	}

	protected abstract Vector2 GetTargetPosition(Vector3 playerPosition, Vector3 targetPosition, float playerEulerY);
	protected abstract void AdjustTargetPositionForRender(ref Vector2 position);

	protected void DrawEnemy(Camera camera, Player enemy, Color playerColor)
	{
		var cameraTransform = camera.transform;
		var cameraPosition = cameraTransform.position;

		var enemyPosition = enemy.Transform.position;
		var cameraEulerY = cameraTransform.eulerAngles.y;

		var enemyMap = GetTargetPosition(cameraPosition, enemyPosition, cameraEulerY);
		var enemyLookDirection = enemy.LookDirection;

		var enemyOffset = enemyPosition + enemyLookDirection * 8f;
		var playerRealRight = enemy.MovementContext.PlayerRealRight;

		var enemyOffset2 = enemyPosition + enemyLookDirection * 4f + playerRealRight * 2f;
		var enemyOffset3 = enemyPosition + enemyLookDirection * 4f - playerRealRight * 2f;

		var enemyForward = GetTargetPosition(cameraPosition, enemyOffset, cameraEulerY);
		var enemyArrow = GetTargetPosition(cameraPosition, enemyOffset2, cameraEulerY);
		var enemyArrow2 = GetTargetPosition(cameraPosition, enemyOffset3, cameraEulerY);

		AdjustTargetPositionForRender(ref enemyMap);
		AdjustTargetPositionForRender(ref enemyForward);
		AdjustTargetPositionForRender(ref enemyArrow);
		AdjustTargetPositionForRender(ref enemyArrow2);

		Render.DrawLine(enemyMap, enemyForward, 2f, Color.white);
		Render.DrawLine(enemyArrow, enemyForward, 2f, Color.white);
		Render.DrawLine(enemyArrow2, enemyForward, 2f, Color.white);
		Render.DrawCircle(enemyMap, 10f, playerColor, 2f, 8);
	}

	protected static string GetHeadingAngle(Vector3 direction)
	{
		var heading = Quaternion.LookRotation(direction).eulerAngles.y;
		return _directions[(int)Mathf.Round(heading % 360 / 45)];
	}
}
