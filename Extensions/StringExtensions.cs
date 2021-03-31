namespace EFT.Trainer.Extensions
{
	public static class StringExtensions
	{
		private static string Color(this string str, string color)
		{
			return $"<color={color.ToLower()}>{str}</color>";
		}

		public static string Blue(this string str)
		{
			return str.Color(nameof(Blue));
		}

		public static string Yellow(this string str)
		{
			return str.Color(nameof(Yellow));
		}

		public static string Red(this string str)
		{
			return str.Color(nameof(Red));
		}

		public static string Green(this string str)
		{
			return str.Color(nameof(Green));
		}
	}
}
