using EFT.InventoryLogic;

namespace EFT.Trainer.Extensions
{
	public static class ItemExtension
	{
		public static bool IsValid(this Item item)
		{
			return item?.Template != null;
		}
	}
}
