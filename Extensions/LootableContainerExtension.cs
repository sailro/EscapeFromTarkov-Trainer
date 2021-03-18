using EFT.Interactive;

namespace EFT.Trainer.Extensions
{
	public static class LootableContainerExtension
	{
		public static bool IsValid(this LootableContainer lootableContainer)
		{
			return lootableContainer != null 
			       && lootableContainer.Template != null;
		}
	}
}
