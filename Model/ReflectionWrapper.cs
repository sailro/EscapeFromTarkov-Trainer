using System;
using System.Collections.Generic;
using System.Reflection;
using EFT.UI;
using HarmonyLib;

#nullable enable

namespace EFT.Trainer.Model
{
	internal class ReflectionWrapper
	{
		private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> _fieldInfoCache = new();

		private readonly Type _instanceType;
		private readonly object _instance;

		protected T? GetFieldValue<T>(string name)
		{
			var fieldInfo = GetField(name);
			if (fieldInfo == null)
			{
#if DEBUG
				AddConsoleLog($"Unable to find {name} on {_instanceType.Name}");
#endif
				return default;
			}

			var value = fieldInfo.GetValue(_instance);
			if (value == null)
				return default;

			return (T)value;
		}

		protected FieldInfo? GetField(string name)
		{
			var infos = _fieldInfoCache[_instanceType];
			if (infos.TryGetValue(name, out var result))
				return result;

			result = AccessTools.Field(_instanceType, name);
			infos.Add(name, result);

			return result;
		}

		protected void AddConsoleLog(string log, string? from = null)
		{
			if (PreloaderUI.Instantiated)
				PreloaderUI.Instance.Console.AddLog(log, from ?? "wrapper");
		}

		public ReflectionWrapper(object instance)
		{
			_instance = instance;
			_instanceType = instance.GetType();

			if (!_fieldInfoCache.TryGetValue(_instanceType, out _))
				_fieldInfoCache.Add(_instanceType, new Dictionary<string, FieldInfo>());
		}
	}
}
