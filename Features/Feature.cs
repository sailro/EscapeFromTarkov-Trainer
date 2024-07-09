using System;
using EFT.InputSystem;
using EFT.UI;
using Newtonsoft.Json;

#nullable enable

namespace EFT.Trainer.Features;

internal interface IFeature
{
	[JsonIgnore]
	public string Name { get; }
}

internal abstract class Feature : InputNode, IFeature
{
	public abstract string Name { get; }
	public abstract string Description { get; }

	private string? _harmonyId = null;

	public void HarmonyPatchOnce(Action<HarmonyLib.Harmony> action)
	{
		if (_harmonyId != null) // this is faster than calling HarmonyLib.Harmony.HasAnyPatches(_harmonyId) for every Update
			return;

		_harmonyId = GetType().FullName;
		var harmony = new HarmonyLib.Harmony(_harmonyId);
		action(harmony);
	}

	public void HarmonyPrefix(HarmonyLib.Harmony harmony, Type originalType, string originalMethod, string newMethod, Type[]? parameters = null)
	{
		var original = HarmonyLib.AccessTools.Method(originalType, originalMethod, parameters);
		if (original == null)
		{
			AddConsoleLog(string.Format(Properties.Strings.ErrorCannotFindOriginalMethodFormat, $"{originalType}.{originalMethod}"));
			return;
		}

		var prefix = HarmonyLib.AccessTools.Method(GetType(), newMethod);
		if (prefix == null)
		{
			AddConsoleLog(string.Format(Properties.Strings.ErrorCannotFindPrefixMethodFormat, newMethod));
			return;
		}

		harmony.Patch(original, prefix: new HarmonyLib.HarmonyMethod(prefix));
#if DEBUG
		AddConsoleLog(string.Format(Properties.Strings.DebugPatchedMethodFormat, $"{originalType}.{originalMethod}", $"{GetType()}.{newMethod}"));
#endif
	}

	public void HarmonyPostfix(HarmonyLib.Harmony harmony, Type originalType, string originalMethod, string newMethod)
	{
		var original = HarmonyLib.AccessTools.Method(originalType, originalMethod);
		if (original == null)
		{
			AddConsoleLog(string.Format(Properties.Strings.ErrorCannotFindOriginalMethodFormat, $"{originalType}.{originalMethod}"));
			return;
		}

		var postfix = HarmonyLib.AccessTools.Method(GetType(), newMethod);
		if (postfix == null)
		{
			AddConsoleLog(string.Format(Properties.Strings.ErrorCannotFindPostfixMethodFormat, newMethod));
			return;
		}

		harmony.Patch(original, postfix: new HarmonyLib.HarmonyMethod(postfix));
#if DEBUG
		AddConsoleLog(string.Format(Properties.Strings.DebugPatchedMethodFormat, $"{originalType}.{originalMethod}", $"{GetType()}.{newMethod}"));
#endif
	}

	protected void AddConsoleLog(string log)
	{
		if (PreloaderUI.Instantiated)
			ConsoleScreen.Log(log);
	}

	public override ETranslateResult TranslateCommand(ECommand command)
	{
		return ETranslateResult.Ignore;
	}

	public override void TranslateAxes(ref float[] axes)
	{
	}

	public override ECursorResult ShouldLockCursor()
	{
		return ECursorResult.Ignore;
	}
}
