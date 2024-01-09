namespace EFT.Trainer;

public static class KnownTemplateIds
{
	//public const string MultiTool = "590c2e1186f77425357b6124";
	public const string BuriedBarrelCache = "5d6d2bb386f774785b07a77a";
	public const string GroundCache = "5d6d2b5486f774785c2ba8ea";
	//public const string Roubles = "5449016a4bdc2d6f028b456f";
	//public const string Euros = "569668774bdc2da2298b4568";
	//public const string Dollars = "5696686a4bdc2da3298b456a";
	public const string Pockets = "557ffd194bdc2d28148b457f";
	public const string DefaultInventory = "55d7217a4bdc2d86028b456d";


	//air drop id. After testing, only the common one is used even the air drop is the other type.
	public const string AirDropCommon = "6223349b3136504a544d1608";
	public const string AirDropMedical = "622334c873090231d904a9fc";
	public const string AirDropSupply = "622334fa3136504a544d160c";
	public const string AirDropWeapon = "6223351bb5d97a7b2c635ca7";

	public static string DefaultInventoryLocalizedShortName = DefaultInventory.LocalizedShortName();
}
