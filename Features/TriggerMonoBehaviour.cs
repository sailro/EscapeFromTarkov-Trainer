using EFT.Trainer.Configuration;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	public class TriggerMonoBehaviour : MonoBehaviour
	{
		[ConfigurationProperty(Order = 2)]
		public virtual KeyCode Key { get; set; } = KeyCode.None;

		private void Update()
		{
			if (Key != KeyCode.None && Input.GetKeyUp(Key))
				UpdateWhenTriggered();
		}

		protected virtual void UpdateWhenTriggered() {}
	}
}
