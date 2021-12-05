using System.Reflection;

#nullable enable

namespace EFT.Trainer.Configuration
{
	internal class OrderedProperty
	{
		public ConfigurationPropertyAttribute Attribute { get; }
		public PropertyInfo Property { get; }
		public string AsString { get; set; }

		public OrderedProperty(ConfigurationPropertyAttribute attribute, PropertyInfo property, string asString = "")
		{
			Attribute = attribute;
			Property = property;
			AsString = asString;
		}
	}
}
