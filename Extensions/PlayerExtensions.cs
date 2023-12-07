using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EFT.InventoryLogic;

#nullable enable

namespace EFT.Trainer.Extensions;

public static class PlayerExtensions
{
	public static bool IsValid([NotNullWhen(true)] this Player? player)
	{
		return player != null
		       && player.Transform != null
		       && player.Transform.Original != null
		       && player.PlayerBones != null
		       && player.PlayerBones.transform != null
		       && player.PlayerBody != null
		       && player.PlayerBody.BodySkins != null;
	}

	public static bool IsAlive([NotNullWhen(true)] this Player? player)
	{
		if (!IsValid(player))
			return false;

		return player.HealthController is {IsAlive: true};
	}

	public static bool HasItemComponentInSlot<T>(this Player? player, EquipmentSlot slot) where T : class, IItemComponent
	{
		if (!IsValid(player))
			return false;

		var playerSlotItem = player.Profile?.Inventory?.Equipment?.GetSlot(slot)?.ContainedItem;
		if (playerSlotItem == null)
			return false;

		return playerSlotItem
			.GetAllItems()
			.GetComponents<T>()
			.Any();
	}
}
