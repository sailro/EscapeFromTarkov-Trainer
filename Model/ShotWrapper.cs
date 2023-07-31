#nullable enable

using EFT.InventoryLogic;
using HarmonyLib;

namespace EFT.Trainer.Model
{
	internal class ShotWrapper : ReflectionWrapper
	{
		public IAIDetails? Player
		{
			get
			{
				var iface = GetFieldValue<object>(nameof(Player));
				if (iface == null)
					return null;

				var field = AccessTools.Field(iface.GetType(), "i" + nameof(Player));
				if (field == null)
					return null;

				return field.GetValue(iface) as IAIDetails;
			}
		} 
		public Item? Weapon => GetFieldValue<Item>(nameof(Weapon));
		public Item? Ammo => GetFieldValue<Item>(nameof(Ammo));

		public ShotWrapper(object instance) : base(instance)
		{
		}
	}
}
