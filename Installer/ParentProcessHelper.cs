using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System;

namespace Installer;

[StructLayout(LayoutKind.Sequential), System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006")]
public partial struct ParentProcessHelper
{
    // These members must match PROCESS_BASIC_INFORMATION
    internal IntPtr Reserved1;
    internal IntPtr PebBaseAddress;
    internal IntPtr Reserved2_0;
    internal IntPtr Reserved2_1;
    internal IntPtr UniqueProcessId;
    internal IntPtr InheritedFromUniqueProcessId;

    [LibraryImport("ntdll.dll")]
    private static partial int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref ParentProcessHelper processInformation, int processInformationLength, out int returnLength);

    public static Process? GetParentProcess()
    {
        return GetParentProcess(Process.GetCurrentProcess().Handle);
    }

    public static Process? GetParentProcess(IntPtr handle)
    {
	    var pp = new ParentProcessHelper();
        int status = NtQueryInformationProcess(handle, 0, ref pp, Marshal.SizeOf(pp), out _);
        if (status != 0)
            throw new Win32Exception(status);

        try
        {
            return Process.GetProcessById(pp.InheritedFromUniqueProcessId.ToInt32());
        }
        catch (ArgumentException)
        {
            // not found
            return null;
        }
    }
}
