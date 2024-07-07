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
			AddConsoleLog($"Cannot find original method {originalType}.{originalMethod}");
			return;
		}

		var prefix = HarmonyLib.AccessTools.Method(GetType(), newMethod);
		if (prefix == null)
		{
			AddConsoleLog($"Cannot find prefix method {newMethod}");
			return;
		}

		harmony.Patch(original, prefix: new HarmonyLib.HarmonyMethod(prefix));
#if DEBUG
		AddConsoleLog($"Patched {originalType}.{originalMethod} with {GetType()}.{newMethod}");
#endif
	}

	public void HarmonyPostfix(HarmonyLib.Harmony harmony, Type originalType, string originalMethod, string newMethod)
	{
		var original = HarmonyLib.AccessTools.Method(originalType, originalMethod);
		if (original == null)
		{
			AddConsoleLog($"Cannot find original method {originalType}.{originalMethod}");
			return;
		}

		var postfix = HarmonyLib.AccessTools.Method(GetType(), newMethod);
		if (postfix == null)
		{
			AddConsoleLog($"Cannot find postfix method {newMethod}");
			return;
		}

		harmony.Patch(original, postfix: new HarmonyLib.HarmonyMethod(postfix));
#if DEBUG
		AddConsoleLog($"Patched {originalType}.{originalMethod} with {GetType()}.{newMethod}");
#endif
	}

	protected void AddConsoleLog(string log)
	{
		if (PreloaderUI.Instantiated)
			ConsoleScreen.Log(log);
	}

	protected override ETranslateResult TranslateCommand(ECommand command)
	{
		return ETranslateResult.Ignore;
	}

	protected override void TranslateAxes(ref float[] axes)
	{
	}

	protected override ECursorResult ShouldLockCursor()
	{
		return ECursorResult.Ignore;
	}
}
