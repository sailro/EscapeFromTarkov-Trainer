using System;
using System.Linq;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.UI
{
	public class GuiEnumDropdown<T> where T : struct, IConvertible
	{

		public GuiEnumDropdown() 
		{
			Selection = Candidates.First();
		}

		public GuiEnumDropdown(T value) 
		{
			Selection = value;
		}

		public T Selection { get; set; }

		private T[]? _candidates = null;
		public T[] Candidates
		{
			get 
			{
				return _candidates ??= Enum.GetValues(typeof(T)).OfType<T>().ToArray();
			}
		}

		private bool 	_isSelecting = false;
		private Vector2 _scrollViewPosition = new();

		public GUILayoutOption[] SelectorOption { get; set; } = Array.Empty<GUILayoutOption>();
		public GUILayoutOption[] CandidateListOption { get; set; } = Array.Empty<GUILayoutOption>();
		public GUILayoutOption[] CandidateButtonOption { get; set; } = Array.Empty<GUILayoutOption>();

		public T EnumDropDown() 
		{
			GUILayout.BeginVertical();

			if(GUILayout.Button(Selection.ToString(),GUI.skin.box,SelectorOption))
				_isSelecting = !_isSelecting;

			if(_isSelecting)
			{
				_scrollViewPosition = GUILayout.BeginScrollView(_scrollViewPosition,GUI.skin.box,CandidateListOption);

				foreach (var candidate in Candidates)
				{
					if (!GUILayout.Button(candidate.ToString(), GUI.skin.label, CandidateButtonOption))
						continue;

					_isSelecting = false;
					Selection = candidate;
				}

				GUILayout.EndScrollView();
			}

			GUILayout.EndVertical();

			return Selection;
		}
	}}
