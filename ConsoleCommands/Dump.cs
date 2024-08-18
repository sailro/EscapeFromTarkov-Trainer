using System;
using System.IO;
using EFT.Trainer.Features;
using EFT.Trainer.Properties;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

#nullable enable

namespace EFT.Trainer.ConsoleCommands;

[UsedImplicitly]
internal class Dump : ConsoleCommandWithoutArgument
{
	public override string Name => Strings.CommandDump;

	public override void Execute()
	{
		var dumpfolder = Path.Combine(Context.UserPath, "Dumps");
		var thisDump = Path.Combine(dumpfolder, $"{DateTime.Now:yyyyMMdd-HHmmss}");

		Directory.CreateDirectory(thisDump);

		AddConsoleLog(Strings.CommandDumpScenes);
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			var scene = SceneManager.GetSceneAt(i);
			if (!scene.isLoaded)
				continue;

			var json = SceneDumper.DumpScene(scene).ToPrettyJson();
			File.WriteAllText(Path.Combine(thisDump, GetSafeFilename($"@scene - {scene.name}.txt")), json);
		}

		AddConsoleLog(Strings.CommandDumpGameObjects);
		foreach (var go in UnityEngine.Object.FindObjectsOfType<GameObject>())
		{
			if (go == null || go.transform.parent != null || !go.activeSelf)
				continue;

			var filename = GetSafeFilename(go.name + "-" + go.GetHashCode() + ".txt");
			var json = SceneDumper.DumpGameObject(go).ToPrettyJson();
			File.WriteAllText(Path.Combine(thisDump, filename), json);
		}

		AddConsoleLog(string.Format(Strings.CommandDumpSuccessFormat, thisDump));
	}

	private static string GetSafeFilename(string filename)
	{
		return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
	}

}
