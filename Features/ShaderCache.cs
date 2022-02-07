using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	internal class ShaderCache : MonoBehaviour
	{
		public Dictionary<Renderer, Shader?> Cache { get; } = new();

		[UsedImplicitly]
		public void OnDestroy()
		{
			Cache.Clear();
		}
	}
}
