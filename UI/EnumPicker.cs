using System;
using System.Linq;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.UI
{
	public class EnumPicker<T> : Picker<T> where T : struct, IConvertible
	{
		private Rect _windowRect = new(20, 20, 200, 500);
		private Vector2 _scrollViewPosition = new();

		public EnumPicker(T value) : base(value)
		{
		}

		public override bool IsSelected { get; protected set; } = false;

		private T[]? _candidates = null;

		public T[] Candidates
		{
			get
			{
				return _candidates ??= Enum
					.GetValues(typeof(T))
					.OfType<T>()
					.OrderBy(i => i)
					.ToArray();
			}
		}

		public override void SetWindowPosition(float x, float y)
		{
			_windowRect.x = x;
			_windowRect.y = y;
		}

		public override void DrawWindow(int id, string title)
		{
			_windowRect = GUI.Window(id, _windowRect, DrawEnumPickerWindow, title);
		}

		private void DrawEnumPickerWindow(int id)
		{
			DrawEnumPicker();
			GUI.DragWindow();
		}

		public void DrawEnumPicker()
		{
			GUILayout.BeginVertical();

			_scrollViewPosition = GUILayout.BeginScrollView(_scrollViewPosition, GUI.skin.box);

			foreach (var candidate in Candidates)
			{
				if (!GUILayout.Button(candidate.ToString(), GUI.skin.label))
					continue;

				IsSelected = true;
				_value = candidate;
			}

			GUILayout.EndScrollView();
			GUILayout.EndVertical();
		}
	}
}
