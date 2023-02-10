using System;
using Comfort.Common;
using EFT.Ballistics;
using EFT.InventoryLogic;
using EFT.Trainer.Extensions;
using EFT.Trainer.Model;
using JetBrains.Annotations;

#nullable enable

namespace EFT.Trainer.Features
{
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
			if (!player.IsValid() || !player.IsYourPlayer)
				return;

			var magazine = weapon.GetCurrentMagazine();
			if (magazine == null) 
				return;

			var instantiated = Singleton<ItemFactory>.Instantiated;
			if (!instantiated)
				return;

			var instance = Singleton<ItemFactory>.Instance;
			var itemId = Guid.NewGuid().ToString("N").Substring(0, 24);
			var cartridges = magazine.Cartridges;

			cartridges?.Add(instance.CreateItem(itemId, ammo.TemplateId, null) ?? ammo, false);
		}

		protected override void UpdateWhenEnabled()
		{
			HarmonyPatchOnce(harmony =>
			{
				var original = HarmonyLib.AccessTools.Method(typeof(BallisticsCalculator), nameof(BallisticsCalculator.Shoot));
				if (original == null)
					return;

				var postfix = HarmonyLib.AccessTools.Method(GetType(), nameof(ShootPostfix));
				if (postfix == null)
					return;

				harmony.Patch(original, postfix: new HarmonyLib.HarmonyMethod(postfix));
			});
		}
	}
}
