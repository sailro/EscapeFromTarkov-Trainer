using EFT.Trainer.Configuration;
using JetBrains.Annotations;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	internal abstract class ToggleFeature : Feature
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

		[UsedImplicitly]
		private void OnGUI()
		{
			if (Enabled)
				OnGUIWhenEnabled();
		}

		protected virtual void UpdateWhenEnabled() {}
		protected virtual void OnGUIWhenEnabled() {}
	}
}
