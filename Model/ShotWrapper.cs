#nullable enable

using EFT.InventoryLogic;

namespace EFT.Trainer.Model
{
	internal class ShotWrapper : ReflectionWrapper
	{
		public Player? Player => GetFieldValue<Player>(nameof(Player));
		public Item? Weapon => GetFieldValue<Item>(nameof(Weapon));
		public Item? Ammo => GetFieldValue<Item>(nameof(Ammo));

		public ShotWrapper(object instance) : base(instance)
		{
		}
	}
}
