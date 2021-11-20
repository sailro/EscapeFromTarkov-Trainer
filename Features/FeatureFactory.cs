using System;
using System.Linq;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	internal static class FeatureFactory
	{
		private static GameObject? _gameObject = null;
		private static readonly Lazy<Type[]> _types = new(() => typeof(FeatureFactory)
			.Assembly
			.GetTypes()
			.Where(t => t.IsSubclassOf(typeof(Feature)) && !t.IsAbstract)
			.ToArray());

		public static Feature[] RegisterAllFeatures(GameObject gameObject)
		{
			_gameObject = gameObject;

			return GetAllFeatureTypes()
				.Select(gameObject.GetOrAddComponent)
				.OfType<Feature>()
				.ToArray();
		}

		public static Type[] GetAllFeatureTypes()
		{
			return _types.Value;
		}

		public static T? GetFeature<T>() where T : Feature
		{
			return GetAllFeatures()
				.OfType<T>()
				.FirstOrDefault();
		}

		public static Feature[] GetAllFeatures()
		{
			if (_gameObject == null)
				return Array.Empty<Feature>();

			return GetAllFeatureTypes()
				.Select(_gameObject.GetComponent)
				.OfType<Feature>()
				.ToArray();
		}

		public static ToggleFeature[] GetAllToggleableFeatures()
		{
			return GetAllFeatures()
				.OfType<ToggleFeature>()
				.ToArray();
		}
	}
}
