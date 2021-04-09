using System;

#nullable enable

namespace EFT.Trainer.Configuration
{
	[AttributeUsage(AttributeTargets.Property)]
	public class ConfigurationPropertyAttribute : Attribute
	{
		public bool Skip { get; set; } = false;
	}
}
