namespace Installer
{
	internal enum ExitCode
	{
		Success = 0,
		NoInstallationFound = 1,
		CompilationFailed = 2,
		Canceled = 3,
		CreateDllFailed = 4,
		CreateOutlineFailed = 5,
		Failure = 6,
		RemoveDllFailed = 7,
		RemoveOutlineFailed = 8,
	}
}
