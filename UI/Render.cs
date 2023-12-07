using UnityEngine;

#nullable enable

namespace EFT.Trainer.UI;

public static class Render
{
	private static GUIStyle? _stringStyle = null;
	private static GUIStyle StringStyle
	{
		get
		{
			// delay initialize stringStyle to be sure we are called under OnGui
			return _stringStyle ??= new GUIStyle(GUI.skin.label);
		}
	}

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

	public static void DrawPlayer(Vector2 position, float size, Color color, float thickness)
	{
		var forward = new Vector2(position.x, position.y - size * 2.5f);
		DrawCircle(position, size, color, thickness, 8);
		DrawLine(position, forward, thickness, color);
		DrawLine(new Vector2(position.x - size / 2, position.y - size * 1.25f), forward, thickness, color);
		DrawLine(new Vector2(position.x + size / 2, position.y - size * 1.25f), forward, thickness, color);
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
		float pivot = /* 180/PI */ Mathf.Rad2Deg * Mathf.Atan(vector.y / vector.x);
		if (vector.x < 0f)
			pivot += 180f;

		thickness = Mathf.Max(thickness, 1f);
		int yOffset = (int)Mathf.Ceil(thickness / 2);

		GUIUtility.RotateAroundPivot(pivot, lineStart);
		GUI.DrawTexture(new Rect(lineStart.x, lineStart.y - yOffset, vector.magnitude, thickness), Texture2D.whiteTexture);
		GUIUtility.RotateAroundPivot(-pivot, lineStart);
	}

	public static void DrawCircle(Vector2 center, float radius, Color color, float width, int segmentsPerQuarter)
	{
		int totalSegments = segmentsPerQuarter * 4;
		float step = 1f / totalSegments;
		var lastV = center + new Vector2(radius, 0);

		for (int i = 1; i <= totalSegments; ++i)
		{
			float t = i * step;
			var currentV = center + new Vector2(
				radius * Mathf.Cos(2 * Mathf.PI * t),
				radius * Mathf.Sin(2 * Mathf.PI * t)
			);
			DrawLine(lastV, currentV, width, color);
			lastV = currentV;
		}
	}
}
