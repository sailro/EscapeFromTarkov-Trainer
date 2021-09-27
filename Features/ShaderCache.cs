using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	public class ShaderCache : MonoBehaviour
	{
		public Dictionary<Material, Shader?> Cache { get; } = new();

		public void OnDestroy()
		{
			Cache.Clear();
		}
	}
}
