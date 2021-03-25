using EFT.Trainer.Configuration;
using UnityEngine;

namespace EFT.Trainer.Features
{
	public abstract class EnableableMonoBehaviour : MonoBehaviour, IEnableable
	{
		[ConfigurationProperty]
		public abstract bool Enabled { get; set; }
	}
}
