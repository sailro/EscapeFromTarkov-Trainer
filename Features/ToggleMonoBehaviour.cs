using EFT.Trainer.Configuration;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	public abstract class ToggleMonoBehaviour : MonoBehaviour
	{
		[ConfigurationProperty(Order = 1)]
		public virtual bool Enabled { get; set; } = true;

		[ConfigurationProperty(Order = 2)]
		public virtual KeyCode Key { get; set; } = KeyCode.None;

		protected virtual void Update()
		{
			if (Key != KeyCode.None && Input.GetKeyUp(Key))
				Enabled = !Enabled;

			if (Enabled)
				UpdateWhenEnabled();
		}

		private void OnGUI()
		{
			if (Enabled)
				OnGUIWhenEnabled();
		}

		protected virtual void UpdateWhenEnabled() {}
		protected virtual void OnGUIWhenEnabled() {}
	}
}
