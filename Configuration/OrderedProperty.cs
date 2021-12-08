using System.Reflection;

#nullable enable

namespace EFT.Trainer.Configuration
{
	internal class OrderedProperty
	{
		public ConfigurationPropertyAttribute Attribute { get; }
		public PropertyInfo Property { get; }

		public OrderedProperty(ConfigurationPropertyAttribute attribute, PropertyInfo property)
		{
			Attribute = attribute;
			Property = property;
		}
	}
}
