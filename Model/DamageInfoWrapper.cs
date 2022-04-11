using EFT.InventoryLogic;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Model
{
	internal class DamageInfoWrapper : ReflectionWrapper
	{
		public float ArmorDamage => GetFieldValue<float>(nameof(ArmorDamage));
		public float Damage => GetFieldValue<float>(nameof(Damage));
		public Vector3 HitPoint => GetFieldValue<Vector3>(nameof(HitPoint));
		public Player? Player => GetFieldValue<Player>(nameof(Player));
		public Item? Weapon => GetFieldValue<Item>(nameof(Weapon));

		public DamageInfoWrapper(object instance) : base(instance)
		{
		}
	}
}
