namespace Installer;

internal enum ExitCode
{
	Success = 0,
	NoInstallationFound = 1,
	CompilationFailed = 2,
	Canceled = 3,
	PluginCompilationFailed = 4,
	Failure = 6,

	CreateDllFailed = 10,
	CreateOutlineFailed = 11,
	CreateHarmonyDllFailed = 12,
	CreatePluginDllFailed = 13,

	RemoveDllFailed = 20,
	RemoveOutlineFailed = 21,
	RemoveHarmonyDllFailed = 22,
	RemovePluginDllFailed = 23,
}
