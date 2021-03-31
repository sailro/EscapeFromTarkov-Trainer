using EFT.Trainer.Configuration;
using UnityEngine;

namespace EFT.Trainer.Features
{
	public abstract class ToggleMonoBehaviour : MonoBehaviour
	{
		[ConfigurationProperty]
		public virtual bool Enabled { get; set; } = true;

		[ConfigurationProperty]
		public virtual KeyCode Key { get; set; } = KeyCode.None;

		private void Update()
		{
			if (Key != KeyCode.None && Input.GetKeyUp(Key))
				Enabled = !Enabled;

			if (Enabled)
				UpdateWhenEnabled();
			else
				UpdateWhenDisabled();
		}

		private void OnGUI()
		{
			if (Enabled)
				OnGUIFeature();
		}

		protected virtual void UpdateWhenEnabled() {}
		protected virtual void UpdateWhenDisabled() {}
		protected virtual void OnGUIFeature() {}
	}
}
