using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#nullable enable

namespace EFT.Trainer.Features
{
	internal class SceneDumper
	{
		[Serializable]
		public class NamedData
		{
			public string? Name;

			public NamedData()
			{
			}

			public override string ToString()
			{
				return Name ?? string.Empty;
			}
		}

		[Serializable]
		public class SceneData : NamedData
		{
			public List<GameObjectData> Roots;

			public SceneData()
			{
				Roots = new List<GameObjectData>();
			}
		}

		[Serializable]
		public class GameObjectData : NamedData
		{
			public string? Tag;

			public List<GameObjectData> Childs;
			public List<ComponentData> Components;

			public GameObjectData()
			{
				Childs = new List<GameObjectData>();
				Components = new List<ComponentData>();
			}
		}

		[Serializable]
		public class ComponentData
		{
			public string? Type;

			public ComponentData()
			{
			}

			public override string ToString()
			{
				return Type ?? string.Empty;
			}
		}

		public static SceneData DumpScene(Scene scene)
		{
			var result = new SceneData { Name = scene.name };

			foreach (var root in scene.GetRootGameObjects())
			{
				if (root != null)
				{
					result.Roots.Add(DumpGameObject(root));
				}
			}

			return result;
		}

		public static GameObjectData DumpGameObject(GameObject root)
		{
			var result = new GameObjectData { Name = root.name, Tag = root.tag };

			foreach (Transform transform in root.transform)
			{
				if (transform != null && transform.gameObject != null)
				{
					result.Childs.Add(DumpGameObject(transform.gameObject));
				}
			}

			foreach (var component in root.GetComponents<Component>())
			{
				if (component != null)
				{
					result.Components.Add(DumpComponent(component));
				}
			}

			return result;
		}

		private static ComponentData DumpComponent(Component component)
		{
			return new() { Type = component.GetType().FullName };
		}
	}
}
