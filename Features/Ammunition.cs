using System;
using Comfort.Common;
using EFT.Ballistics;
using EFT.InventoryLogic;
using EFT.Trainer.Model;
using HarmonyLib;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class Ammunition : ToggleFeature
{
	public override string Name => "ammo";

	public override bool Enabled { get; set; } = false;

	[UsedImplicitly]
	private static void ShootPostfix(object shot)
	{
		var feature = FeatureFactory.GetFeature<Ammunition>();
		if (feature == null || !feature.Enabled)
			return;

		var shotWrapper = new ShotWrapper(shot);
		if (shotWrapper.Weapon is not Weapon weapon)
			return;

		var ammo = shotWrapper.Ammo;
		if (ammo == null)
			return;

		var player = shotWrapper.Player;
		if (player is not { IsYourPlayer: true })
			return;

		var magazine = weapon.GetCurrentMagazine();
		if (magazine != null)
		{
			if (magazine is CylinderMagazineClass cylinderMagazine)
			{
				// Rhino case
				foreach (var slot in cylinderMagazine.Camoras)
					slot.Add(CreateAmmo(ammo), false, true);
			}
			else
			{
				var cartridges = magazine.Cartridges;
				cartridges?.Add(CreateAmmo(ammo), false);
			}
		}
		else
		{
			// no magazine, like mp18, fill all weapon chambers
			foreach (var slot in weapon.Chambers)
				slot.Add(CreateAmmo(ammo), false, true);
		}
	}

	private static Item CreateAmmo(Item ammo)
	{
		var instantiated = Singleton<ItemFactory>.Instantiated;
		if (!instantiated)
			return ammo;

		var instance = Singleton<ItemFactory>.Instance;
		var itemId = Guid.NewGuid().ToString("N").Substring(0, 24);
		return instance.CreateItem(itemId, ammo.TemplateId, null) ?? ammo;
	}

	protected override void UpdateWhenEnabled()
	{
		HarmonyPatchOnce(harmony =>
		{
			var original = AccessTools.Method(typeof(BallisticsCalculator), nameof(BallisticsCalculator.Shoot));
			if (original == null)
				return;

			var postfix = AccessTools.Method(GetType(), nameof(ShootPostfix));
			if (postfix == null)
				return;

			harmony.Patch(original, postfix: new HarmonyMethod(postfix));
		});
	}
}
