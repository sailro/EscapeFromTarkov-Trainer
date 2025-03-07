using System;
using Comfort.Common;
using EFT.Ballistics;
using EFT.InventoryLogic;
using EFT.Trainer.Properties;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.Features;

[UsedImplicitly]
internal class Ammunition : ToggleFeature
{
	public override string Name => Strings.FeatureAmmunitionName;
	public override string Description => Strings.FeatureAmmunitionDescription;

	public override bool Enabled { get; set; } = false;

	[UsedImplicitly]
	private static void ShootPostfix(EftBulletClass shot)
	{
		var feature = FeatureFactory.GetFeature<Ammunition>();
		if (feature == null || !feature.Enabled)
			return;

		if (shot.Weapon is not Weapon weapon)
			return;

		var ammo = shot.Ammo;
		if (ammo == null)
			return;

		var player = shot.Player.iPlayer;
		if (player is not { IsYourPlayer: true })
			return;

		var magazine = weapon.GetCurrentMagazine();
		if (magazine != null)
		{
			if (magazine is CylinderMagazineItemClass cylinderMagazine)
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
		var instantiated = Singleton<ItemFactoryClass>.Instantiated;
		if (!instantiated)
			return ammo;

		var instance = Singleton<ItemFactoryClass>.Instance;
		var itemId = Guid.NewGuid().ToString("N").Substring(0, 24);
		return instance.CreateItem(itemId, ammo.TemplateId, null) ?? ammo;
	}

	protected override void UpdateWhenEnabled()
	{
		HarmonyPatchOnce(harmony =>
		{
			HarmonyPostfix(harmony, typeof(BallisticsCalculator), nameof(BallisticsCalculator.Shoot), nameof(ShootPostfix), [typeof(EftBulletClass)]);
		});
	}
}
