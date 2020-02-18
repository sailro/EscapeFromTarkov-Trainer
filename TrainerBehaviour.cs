using System;
using UnityEngine;
using System.Collections.Generic;
using EFT.InventoryLogic;
using System.IO;
using UnityEngine.SceneManagement;
using EFT.Interactive;
using System.Diagnostics;
using Comfort.Common;

namespace EFT.Trainer
{
	public class TrainerBehaviour : MonoBehaviourSingleton<TrainerBehaviour>
	{
		private readonly Dictionary<KeyCode, Action> _actions = new Dictionary<KeyCode, Action>();
		private Shader _outline;
		private string _hud = string.Empty;
		private readonly Stopwatch _watch = new Stopwatch();

		public void Start()
		{
			_actions.Clear();

			_actions.Add(KeyCode.KeypadPlus, DumpScenes);

			var bundle = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath, "outline"));
			_outline = bundle.LoadAsset<Shader>("assets/outline.shader");
			//standard = Shader.Find("p0/Reflective/Bumped Specular SMap");

			InvokeRepeating("Trainer", 10f, 1f);
		}

		private static void DumpScenes()
		{
			for (int i = 0; i < SceneManager.sceneCount; i++) 
			{
				var scene = SceneManager.GetSceneAt(i);
				if (!scene.isLoaded)
					continue;

				var json = SceneDumper.DumpScene(scene).ToPrettyJson();
				File.WriteAllText(Path.Combine(Application.dataPath, "Dump", $"@scene - {scene.name}.txt"), json);
			}

			foreach (var go in FindObjectsOfType<GameObject>())
			{
				if (go == null || go.transform.parent != null || !go.activeSelf) 
					continue;

				var filename = go.name + "-" + go.GetHashCode() + ".txt";
				var json = SceneDumper.DumpGameObject(go).ToPrettyJson();
				File.WriteAllText(Path.Combine(Application.dataPath, "Dump", filename), json);
			}
		}

		public void OnGUI()
		{
			var textStyle = new GUIStyle(GUI.skin.label) {fontStyle = FontStyle.Bold, fontSize = 32};
			GUI.Label(new Rect(512, Screen.height - 48, 512, 48), _hud, textStyle);
		}

		public void Trainer()
		{
			_watch.Restart();
			_hud = "Trainer loaded!";
			var world = Singleton<GameWorld>.Instance;

			try
			{
				UnlockDoors();

				var bones = FindObjectsOfType<PlayerBones>();
				foreach(var playerBones in bones)
				{
					var go = playerBones.transform.parent;
					var player = playerBones?.Player;

					if (player != null && player.IsYourPlayer())
					{
						DisableBulletHits(go);
						AutoHealth(player);
						PrepareHud(player);
						OutlineExfiltrationPoints(world, player);
					}
					else
					{
						OutlinePlayer(player, go);
					}
				}
			}
			finally
			{
				_watch.Stop();
				_hud += $" ({_watch.ElapsedMilliseconds} ms)";
			}
		}

		private static void UnlockDoors()
		{
			var doors = FindObjectsOfType<Door>();
			foreach (var door in doors)
			{
				// door unlocker
				if (door == null)
					continue;

				if (door.DoorState == EDoorState.Locked)
					door.DoorState = EDoorState.Shut;
			}
		}

		private void OutlinePlayer(Player player, Transform go)
		{
			// wallhack
			var color = player != null ? Color.green : Color.cyan;
			SetShaders(go, _outline, color);
		}

		private void OutlineExfiltrationPoints(GameWorld world, Player player)
		{
			var ect = world?.ExfiltrationController;
			if (ect?.ExfiltrationPoints == null)
				return;

			// Exfiltration points
			var profile = player.Profile;
			var side = profile.Info?.Side;

			int scavMask = 0;
			if (player is ClientPlayer clientPlayer)
				scavMask = clientPlayer.ScavExfilMask;

			var points = side == EPlayerSide.Savage ? ect.ScavExfiltrationClaim(scavMask, profile.Id) : ect.EligiblePoints(profile);
			foreach (var point in points)
			{
				if (point.transform.Find(nameof(TrainerBehaviour)) != null)
					continue;

				var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				sphere.transform.position = point.transform.position;

				var collider = sphere.GetComponent<SphereCollider>();
				collider.isTrigger = true;

				sphere.transform.parent = point.transform;
				sphere.name = nameof(TrainerBehaviour);

				SetShaders(point.transform, _outline, Color.magenta, true);
			}
		}

		private void PrepareHud(Player player)
		{
			// ammo hud
			if (player.HandsController == null || !(player.HandsController.Item is Weapon))
				return;

			var weapon = player.Weapon;

			var mag = weapon?.GetCurrentMagazine();
			if (mag != null)
			{
				_hud = $"{mag.Count}+{weapon.ChamberAmmoCount}/{mag.MaxCount} [{weapon.SelectedFireMode.ToString()}]";
			}
		}

		private static void AutoHealth(Player player)
		{
			if (player.HealthController == null || player.HealthStatus == ETagStatus.Healthy)
				return;

			// auto health (offline only)
			foreach (EBodyPart part in Enum.GetValues(typeof(EBodyPart)))
				player.Heal(part, 100);
		}

		private static void DisableBulletHits(Transform go)
		{
			// disable bullet hit (offline only)
			foreach (var rigidbody in go.GetComponentsInChildren<Rigidbody>())
				rigidbody.detectCollisions = false;
		}

		private static void SetShaders(Transform go, Shader shader, Color color, bool force = false)
		{
			if (go == null)
				return;

			foreach (var renderer in go.GetComponentsInChildren<Renderer>())
			{
				var material = renderer.material;
				var current = material.shader;
				if (!force && (current.name == shader.name || !current.name.StartsWith("p0")))
					continue;

				material.shader = shader;
				material.SetColor("_FirstOutlineColor", Color.red);
				material.SetFloat("_FirstOutlineWidth", 0.02f);

				material.SetColor("_SecondOutlineColor", color);
				material.SetFloat("_SecondOutlineWidth", 0.0025f);
			}
		}

		public void Update()
		{
			foreach (var keyCode in _actions.Keys)
			{
				if (Input.GetKeyDown(keyCode))
					_actions[keyCode]();
			}
		}
	}
}
