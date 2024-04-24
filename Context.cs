using System;
using System.IO;
using System.Linq;
using EFT.Trainer.Features;

namespace EFT.Trainer;

internal static class Context
{
	public static string UserPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Escape from Tarkov");
	public static string ConfigFile => Path.Combine(UserPath, "trainer.ini");

	public static Lazy<Feature[]> Features => new(() => [.. FeatureFactory.GetAllFeatures().OrderBy(f => f.Name)]);
	public static Lazy<ToggleFeature[]> ToggleableFeatures => new(() => [.. FeatureFactory.GetAllToggleableFeatures().OrderByDescending(f => f.Name)]);
}
