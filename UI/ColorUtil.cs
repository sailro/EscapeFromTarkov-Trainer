using UnityEngine;

namespace EFT.Trainer.UI
{
	public class ColorUtil
	{

		public static Color HsvToRgb(float h, float s, float v, bool hdr = false)
		{
			var white = Color.white;
			if (s == 0f)
			{
				white.r = v;
				white.g = v;
				white.b = v;
			}
			else if (v == 0f)
			{
				white.r = 0f;
				white.g = 0f;
				white.b = 0f;
			}
			else
			{
				white.r = 0f;
				white.g = 0f;
				white.b = 0f;
				float num = h * 6f;
				int num2 = (int)Mathf.Floor(num);
				float num3 = num - num2;
				float num4 = v * (1f - s);
				float num5 = v * (1f - s * num3);
				float num6 = v * (1f - s * (1f - num3));
				switch (num2 + 1)
				{
					case 0:
						white.r = v;
						white.g = num4;
						white.b = num5;
						break;
					case 1:
						white.r = v;
						white.g = num6;
						white.b = num4;
						break;
					case 2:
						white.r = num5;
						white.g = v;
						white.b = num4;
						break;
					case 3:
						white.r = num4;
						white.g = v;
						white.b = num6;
						break;
					case 4:
						white.r = num4;
						white.g = num5;
						white.b = v;
						break;
					case 5:
						white.r = num6;
						white.g = num4;
						white.b = v;
						break;
					case 6:
						white.r = v;
						white.g = num4;
						white.b = num5;
						break;
					case 7:
						white.r = v;
						white.g = num6;
						white.b = num4;
						break;
				}

				if (hdr)
					return white;

				white.r = Mathf.Clamp(white.r, 0f, 1f);
				white.g = Mathf.Clamp(white.g, 0f, 1f);
				white.b = Mathf.Clamp(white.b, 0f, 1f);
			}

			return white;
		}

		public static void RgbToHsv(Color color, out float h, out float s, out float v)
		{
			if (color.b > color.g && color.b > color.r)
			{
				RgbToHsvHelper(4f, color.b, color.r, color.g, out h, out s, out v);
			}
			else if (color.g > color.r)
			{
				RgbToHsvHelper(2f, color.g, color.b, color.r, out h, out s, out v);
			}
			else
			{
				RgbToHsvHelper(0f, color.r, color.g, color.b, out h, out s, out v);
			}
		}

		private static void RgbToHsvHelper(float offset, float dominantcolor, float colorone, float colortwo, out float h, out float s, out float v)
		{
			v = dominantcolor;
			if (v != 0f)
			{
				var num = colorone > colortwo ? colortwo : colorone;
				float num2 = v - num;
				if (num2 != 0f)
				{
					s = num2 / v;
					h = offset + (colorone - colortwo) / num2;
				}
				else
				{
					s = 0f;
					h = offset + (colorone - colortwo);
				}

				h /= 6f;
				if (h < 0f)
				{
					h += 1f;
				}
			}
			else
			{
				s = 0f;
				h = 0f;
			}
		}
	}
}
