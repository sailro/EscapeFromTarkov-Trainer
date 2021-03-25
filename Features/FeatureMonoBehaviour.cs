using EFT.Trainer.Configuration;
using UnityEngine;

namespace EFT.Trainer.Features
{
	public abstract class FeatureMonoBehaviour : MonoBehaviour
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
				UpdateFeature();
		}

		private void OnGUI()
		{
			if (Enabled)
				OnGUIFeature();
		}

		protected virtual void UpdateFeature() {}
		protected virtual void OnGUIFeature() {}
	}
}
