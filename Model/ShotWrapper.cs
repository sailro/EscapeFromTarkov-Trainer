using EFT.InventoryLogic;
using HarmonyLib;

#nullable enable

namespace EFT.Trainer.Model;

internal class ShotWrapper(object instance) : ReflectionWrapper(instance)
{
	public IPlayer? Player
	{
		get
		{
			var iface = GetFieldValue<object>(nameof(Player));
			if (iface == null)
				return null;

			var property = AccessTools.Property(iface.GetType(), "i" + nameof(Player));
			if (property == null)
				return null;

			return property.GetValue(iface) as IPlayer;
		}
	} 
	public Item? Weapon => GetFieldValue<Item>(nameof(Weapon));
	public Item? Ammo => GetFieldValue<Item>(nameof(Ammo));
}
