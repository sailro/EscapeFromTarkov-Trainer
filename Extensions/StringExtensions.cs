using UnityEngine;

#nullable enable

namespace EFT.Trainer.Extensions
{
	public static class StringExtensions
	{
		public static string Color(this string str, Color color)
		{
			return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{str}</color>";
		}

		public static string Blue(this string str)
		{
			return str.Color(UnityEngine.Color.blue);
		}

		public static string Yellow(this string str)
		{
			return str.Color(UnityEngine.Color.yellow);
		}

		public static string Red(this string str)
		{
			return str.Color(UnityEngine.Color.red);
		}

		public static string Green(this string str)
		{
			return str.Color(UnityEngine.Color.green);
		}

		public static string Cyan(this string str)
		{
			return str.Color(UnityEngine.Color.cyan);
		}
	}
}
