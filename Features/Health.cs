using System;
using System.Collections.Generic;
using System.Linq;
using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using EFT.Trainer.Model;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	[UsedImplicitly]
	internal class Health : ToggleFeature
	{
		public override string Name => "health";

		public override bool Enabled { get; set; } = false;

		[ConfigurationProperty]
		public bool VitalsOnly { get; set; } = false;

		[ConfigurationProperty]
		public bool RemoveNegativeEffects { get; set; } = true;

		[ConfigurationProperty]
		public bool FoodWater { get; set; } = true;

		private static readonly Array _bodyParts = Enum.GetValues(typeof(EBodyPart));

		private static String[] TargetBones = { "head", "spine3", "spine2", "spine1" };

		[UsedImplicitly]
		protected static bool ApplyDamagePrefix(object? __instance, ref float __result)
		{
			var feature = FeatureFactory.GetFeature<Health>();
			if (feature == null || !feature.Enabled || __instance == null)
				return true; // keep using original code, we are not enabled

			var healthControllerWrapper = new HealthControllerWrapper(__instance);
			var player = healthControllerWrapper.Player;
			
			if (player == null || !player.IsYourPlayer)
				return true; // keep using original code, apply damage to others
			
			if (feature.VitalsOnly)
			{
				__result = 0.01f; // hack to allow damage but not enough to kill you. Should probably look at the body part about to receive damage and set to zero
				return true;
			}
				__result = 0f;
			return false;  // skip the original code and all other prefix methods 
		}

		private static IEnumerable<Transform> EnumerateHierarchyCore(Transform root)
		{
			Queue<Transform> transformQueue = new Queue<Transform>();
			transformQueue.Enqueue(root);

			while (transformQueue.Count > 0)
			{
				Transform parentTransform = transformQueue.Dequeue();

				if (!parentTransform)
				{
					continue;
				}

				for (Int32 i = 0; i < parentTransform.childCount; i++)
				{
					transformQueue.Enqueue(parentTransform.GetChild(i));
				}

				yield return parentTransform;
			}
		}

		protected override void UpdateWhenEnabled()
		{
			var player = GameState.Current?.LocalPlayer;
			if (!player.IsValid())
				return;

			var healthController = player.ActiveHealthController;
			if (healthController == null)
				return;

			HarmonyPatchOnce(harmony =>
			{
				var original = HarmonyLib.AccessTools.Method(healthController.GetType(), nameof(healthController.ApplyDamage));
				if (original == null)
					return;

				var prefix = HarmonyLib.AccessTools.Method(GetType(), nameof(ApplyDamagePrefix));
				if (prefix == null)
					return;

				harmony.Patch(original, new HarmonyLib.HarmonyMethod(prefix));
			});

			if (VitalsOnly)
			{
				foreach (Transform transform in Health.EnumerateHierarchyCore(player.gameObject.transform).Where(t => TargetBones.Any(u => t.name.ToLower().Contains(u))))
				{
					if (transform.gameObject.layer != LayerMask.NameToLayer("PlayerSpiritAura"))
					{
						transform.gameObject.layer = LayerMask.NameToLayer("PlayerSpiritAura"); // Theoretically makes it impossible to hit your head/chest. Can still die from collateral damage.
					}
				}
			}

			foreach (EBodyPart bodyPart in _bodyParts)
			{

				if (bodyPart == EBodyPart.Common)
					continue;

				if (VitalsOnly && !(bodyPart == EBodyPart.Chest || bodyPart == EBodyPart.Head))
					continue;

				if (healthController.IsBodyPartBroken(bodyPart) || healthController.IsBodyPartDestroyed(bodyPart))
					healthController.RestoreBodyPart(bodyPart, 0);

				if (RemoveNegativeEffects)
				{
					healthController.RemoveNegativeEffects(EBodyPart.Common);
					healthController.RemoveNegativeEffects(bodyPart);
				}

				var bodyPartHealth = healthController.GetBodyPartHealth(bodyPart);
				if (bodyPartHealth.AtMaximum)
					continue;

				if (!VitalsOnly)
					healthController.RestoreFullHealth();

				healthController.ChangeHealth(bodyPart, bodyPartHealth.Maximum, default);

				break;
			}

			if (!healthController.Energy.AtMaximum && FoodWater)
				healthController.ChangeEnergy(healthController.Energy.Maximum);

			if (!healthController.Hydration.AtMaximum && FoodWater)
				healthController.ChangeHydration(healthController.Hydration.Maximum);
		}
	}
}
