using UnityEngine;

namespace EFT.Trainer.UI
{
	public class ColorPicker : Picker<Color>
	{

		public float H => _h;
		public float S => _s;
		public float V => _v;

		private float _h = 0f;
		private float _s = 0f;
		private float _v = 0f;

		private Rect _windowRect = new(20, 20, 165, 100);

		private readonly GUIStyle _previewStyle;
		private readonly GUIStyle _svStyle;
		private readonly GUIStyle _hueStyle;

		private readonly Texture2D _svTexture;
		private readonly Texture2D _circle;
		private readonly Texture2D _rightArrow;
		private readonly Texture2D _leftArrow;

		const int HsvPickerSize = 120, HuePickerWidth = 16;

		public ColorPicker(Color color) : base(color)
		{
			ColorUtil.RgbToHsv(Value, out _h, out _s, out _v);

			_circle = Resources.Load<Texture2D>("imCircle");
			_rightArrow = Resources.Load<Texture2D>("imRight");
			_leftArrow = Resources.Load<Texture2D>("imLeft");
			_previewStyle = new GUIStyle {normal = {background = Texture2D.whiteTexture}};

			var hueTexture = CreateHueTexture(20, HsvPickerSize);
			_hueStyle = new GUIStyle {normal = {background = hueTexture}};

			_svTexture = CreateSvTexture(Value, HsvPickerSize);
			_svStyle = new GUIStyle {normal = {background = _svTexture}};
		}

		public override bool IsSelected { get; protected set; } = false; // with this picker we always keep focus, until another control is selected

		public override void SetWindowPosition(float x, float y)
		{
			_windowRect.x = x;
			_windowRect.y = y;
		}

		public override void DrawWindow(int id, string title)
		{
			_windowRect = GUI.Window(id, _windowRect, DrawColorPickerWindow, title);
		}

		private void DrawColorPickerWindow(int id)
		{
			DrawColorPicker();

			if (Event.current.type == EventType.Repaint)
			{
				var rect = GUILayoutUtility.GetLastRect();
				_windowRect.height = rect.y + rect.height + 10f;
			}

			GUI.DragWindow();
		}

		public void DrawColorPicker()
		{
			using (new GUILayout.VerticalScope())
			{
				GUILayout.Space(5f);
				DrawPreview(Value);

				GUILayout.Space(5f);
				DrawHsvPicker(ref _value);
			}
		}

		private void DrawPreview(Color color)
		{
			using (new GUILayout.VerticalScope())
			{
				var tmp = GUI.backgroundColor;
				GUI.backgroundColor = new Color(color.r, color.g, color.b);
				GUILayout.Label(string.Empty, _previewStyle, GUILayout.Width(HsvPickerSize + HuePickerWidth + 10), GUILayout.Height(12f));

				GUILayout.Space(1f);

				var alpha = color.a;
				GUI.backgroundColor = new Color(alpha, alpha, alpha);
				GUILayout.Label(string.Empty, _previewStyle, GUILayout.Width(HsvPickerSize + HuePickerWidth + 10), GUILayout.Height(2f));

				GUI.backgroundColor = tmp;
			}
		}

		private void DrawHsvPicker(ref Color color)
		{
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.Label(string.Empty, _svStyle, GUILayout.Width(HsvPickerSize), GUILayout.Height(HsvPickerSize));
				DrawSvHandler(GUILayoutUtility.GetLastRect(), ref color);

				GUILayout.Space(10f);

				GUILayout.Label(string.Empty, _hueStyle, GUILayout.Width(HuePickerWidth), GUILayout.Height(HsvPickerSize));
				DrawHueHandler(GUILayoutUtility.GetLastRect(), ref color);
			}
		}

		private void DrawSvHandler(Rect rect, ref Color color)
		{
			const float size = 10f;
			const float offset = 5f;
			GUI.DrawTexture(new Rect(rect.x + _s * rect.width - offset, rect.y + (1f - _v) * rect.height - offset, size, size), _circle);

			var e = Event.current;
			var p = e.mousePosition;

			if (e.button != 0 || e.type != EventType.MouseDown && e.type != EventType.MouseDrag || !rect.Contains(p))
				return;

			_s = (p.x - rect.x) / rect.width;
			_v = 1f - (p.y - rect.y) / rect.height;
			color = ColorUtil.HsvToRgb(_h, _s, _v);

			e.Use();
		}

		private void DrawHueHandler(Rect rect, ref Color c)
		{
			const float size = 15f;
			GUI.DrawTexture(new Rect(rect.x - size * 0.75f, rect.y + (1f - _h) * rect.height - size * 0.5f, size, size), _rightArrow);
			GUI.DrawTexture(new Rect(rect.x + rect.width - size * 0.25f, rect.y + (1f - _h) * rect.height - size * 0.5f, size, size), _leftArrow);

			var e = Event.current;
			var p = e.mousePosition;

			if (e.button != 0 || e.type != EventType.MouseDown && e.type != EventType.MouseDrag || !rect.Contains(p))
				return;

			_h = 1f - (p.y - rect.y) / rect.height;
			c = ColorUtil.HsvToRgb(_h, _s, _v);
			UpdateSvTexture(c, _svTexture);

			e.Use();
		}

		private void UpdateSvTexture(Color c, Texture2D tex)
		{
			ColorUtil.RgbToHsv(c, out var h, out _, out _);

			var size = tex.width;
			for (int y = 0; y < size; y++)
			{
				var v = 1f * y / size;
				for (int x = 0; x < size; x++)
				{
					var s = 1f * x / size;
					var color = ColorUtil.HsvToRgb(h, s, v);
					tex.SetPixel(x, y, color);
				}
			}

			tex.Apply();
		}

		private static Texture2D CreateHueTexture(int width, int height)
		{
			var tex = new Texture2D(width, height);
			for (int y = 0; y < height; y++)
			{
				var h = 1f * y / height;
				var color = ColorUtil.HsvToRgb(h, 1f, 1f);
				for (int x = 0; x < width; x++)
				{
					tex.SetPixel(x, y, color);
				}
			}

			tex.Apply();
			return tex;
		}

		private Texture2D CreateSvTexture(Color c, int size)
		{
			var tex = new Texture2D(size, size);
			UpdateSvTexture(c, tex);
			return tex;
		}
	}
}
