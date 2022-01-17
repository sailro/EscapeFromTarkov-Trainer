using UnityEngine;

#nullable enable

namespace EFT.Trainer.UI
{
	public static class Render
	{
		public static GUIStyle StringStyle { get; set; } = new(GUI.skin.label);

		public static Vector2 ScreenCenter => new(Screen.width / 2f, Screen.height / 2f);

		public static Color Color
		{
			get { return GUI.color; }
			set { GUI.color = value; }
		}

		public static Vector2 DrawString(Vector2 position, string label, Color color, bool centered = true)
		{
			Color = color;
			return DrawString(position, label, centered);
		}

		public static void GetContentAndSize(string label, out GUIContent content, out Vector2 size)
		{
			content = new GUIContent(label);
			size = StringStyle.CalcSize(content);
		}

		public static Vector2 DrawString(Vector2 position, string label, bool centered = true)
		{
			GetContentAndSize(label, out var content, out var size);
			var upperLeft = centered ? position - size / 2f : position;
			GUI.Label(new Rect(upperLeft, size), content);
			return size;
		}

		public static void DrawCrosshair(Vector2 position, float size, Color color, float thickness)
		{
			Color = color;
			var texture = Texture2D.whiteTexture;
			GUI.DrawTexture(new Rect(position.x - size, position.y, size * 2 + thickness, thickness), texture);
			GUI.DrawTexture(new Rect(position.x, position.y - size, thickness, size * 2 + thickness), texture);
		}

		public static void DrawBox(float x, float y, float w, float h, float thickness, Color color)
		{
			Color = color;
			var texture = Texture2D.whiteTexture;
			GUI.DrawTexture(new Rect(x, y, w + thickness, thickness), texture);
			GUI.DrawTexture(new Rect(x, y, thickness, h + thickness), texture);
			GUI.DrawTexture(new Rect(x + w, y, thickness, h + thickness), texture);
			GUI.DrawTexture(new Rect(x, y + h, w + thickness, thickness), texture);
		}

		public static void DrawLine(Vector2 lineStart, Vector2 lineEnd, float thickness, Color color)
		{
			Color = color;

			var vector = lineEnd - lineStart;
			float pivot = /* 180/PI */ 57.29578f * Mathf.Atan(vector.y / vector.x);
			if (vector.x < 0f)
				pivot += 180f;

			if (thickness < 1f)
				thickness = 1;

			int yOffset = (int)Mathf.Ceil(thickness / 2);

			GUIUtility.RotateAroundPivot(pivot, lineStart);
			GUI.DrawTexture(new Rect(lineStart.x, lineStart.y - yOffset, vector.magnitude, thickness), Texture2D.whiteTexture);
			GUIUtility.RotateAroundPivot(-pivot, lineStart);
		}

		public static void DrawCircle(Vector2 center, float radius, Color color, float width, int segmentsPerQuarter)
		{
			var rh = radius / 2;

			var p1 = new Vector2(center.x, center.y - radius);
			var p1TanA = new Vector2(center.x - rh, center.y - radius);
			var p1TanB = new Vector2(center.x + rh, center.y - radius);

			var p2 = new Vector2(center.x + radius, center.y);
			var p2TanA = new Vector2(center.x + radius, center.y - rh);
			var p2TanB = new Vector2(center.x + radius, center.y + rh);

			var p3 = new Vector2(center.x, center.y + radius);
			var p3TanA = new Vector2(center.x - rh, center.y + radius);
			var p3TanB = new Vector2(center.x + rh, center.y + radius);

			var p4 = new Vector2(center.x - radius, center.y);
			var p4TanA = new Vector2(center.x - radius, center.y - rh);
			var p4TanB = new Vector2(center.x - radius, center.y + rh);

			DrawBezierLine(p1, p1TanB, p2, p2TanA, color, width, segmentsPerQuarter);
			DrawBezierLine(p2, p2TanB, p3, p3TanB, color, width, segmentsPerQuarter);
			DrawBezierLine(p3, p3TanA, p4, p4TanB, color, width, segmentsPerQuarter);
			DrawBezierLine(p4, p4TanA, p1, p1TanA, color, width, segmentsPerQuarter);
		}

		public static void DrawBezierLine(Vector2 start, Vector2 startTangent, Vector2 end, Vector2 endTangent, Color color, float width, int segments)
		{
			var lastV = CubeBezier(start, startTangent, end, endTangent, 0);
			for (int i = 1; i < segments + 1; ++i)
			{
				var v = CubeBezier(start, startTangent, end, endTangent, i / (float)segments);
				DrawLine(lastV, v, width, color);
				lastV = v;
			}
		}

		private static Vector2 CubeBezier(Vector2 s, Vector2 st, Vector2 e, Vector2 et, float t)
		{
			float rt = 1 - t;
			return rt * rt * rt * s + 3 * rt * rt * t * st + 3 * rt * t * t * et + t * t * t * e;
		}
	}
}
