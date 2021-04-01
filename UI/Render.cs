using UnityEngine;

namespace EFT.Trainer.UI
{
    public static class Render
    {
        public static GUIStyle StringStyle { get; set; } = new(GUI.skin.label);
        public static readonly Rect LineRect = new(0f, 0f, 1f, 1f);
        public static readonly Texture2D Texture = new(2, 2, TextureFormat.ARGB32, false);

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

        public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width)
        {
	        var diffx = pointB.x - pointA.x;
	        var diffy = pointB.y - pointA.y;
	        var sqrt = Mathf.Sqrt(diffx * diffx + diffy * diffy);

	        if (!(sqrt >= 0.001f))
		        return;
	        
	        var sqdy = width * diffy / sqrt;
	        var sqdx = width * diffx / sqrt;
	        
	        var identity = Matrix4x4.identity;
	        identity.m00 = diffx;
	        identity.m01 = -sqdy;
	        identity.m03 = pointA.x + 0.5f * sqdy;
	        identity.m10 = diffy;
	        identity.m11 = sqdx;
	        identity.m13 = pointA.y - 0.5f * sqdx;

	        GL.PushMatrix();
	        GL.MultMatrix(identity);
	        GUI.color = color;
	        GUI.DrawTexture(LineRect, Texture);
	        GL.PopMatrix();
        }
	}
}
