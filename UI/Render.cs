using UnityEngine;

namespace EFT.Trainer.UI
{
    public static class Render
    {
        public static GUIStyle StringStyle { get; set; } = new(GUI.skin.label);

        public static Color Color
        {
            get { return GUI.color; }
            set { GUI.color = value; }
        }

        public static void DrawString(Vector2 position, string label, Color color, bool centered = true)
        {
            Color = color;
            DrawString(position, label, centered);
        }

        public static void DrawString(Vector2 position, string label, bool centered = true)
        {
            var content = new GUIContent(label);
            var size = StringStyle.CalcSize(content);
            var upperLeft = centered ? position - size / 2f : position;
            GUI.Label(new Rect(upperLeft, size), content);
        }

        public static void DrawCrosshair(Vector2 position, float size, Color color, float thickness)
        {
	        Color = color;
	        GUI.DrawTexture(new Rect(position.x - size, position.y, size * 2 + thickness, thickness), Texture2D.whiteTexture);
	        GUI.DrawTexture(new Rect(position.x, position.y - size, thickness, size * 2 + thickness), Texture2D.whiteTexture);
        }
	}
}
