using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using MonoMod.Utils;

namespace BepInEx.Unix;

internal static class UnixStreamHelper
{
    public delegate int dupDelegate(int fd);

    public delegate int fcloseDelegate(IntPtr stream);

    public delegate IntPtr fdopenDelegate(int fd, string mode);

    public delegate int fflushDelegate(IntPtr stream);

    public delegate IntPtr freadDelegate(IntPtr ptr, IntPtr size, IntPtr nmemb, IntPtr stream);

    public delegate int fwriteDelegate(IntPtr ptr, IntPtr size, IntPtr nmemb, IntPtr stream);

    public delegate int isattyDelegate(int fd);

    public static dupDelegate dup;

    public static fdopenDelegate fdopen;

    public static freadDelegate fread;

    public static fwriteDelegate fwrite;

    public static fcloseDelegate fclose;

    public static fflushDelegate fflush;

    public static isattyDelegate isatty;

    static UnixStreamHelper()
    {
        var libcMapping = new List<string>
        {
                "libc.so.6",               // Ubuntu glibc
                "libc",                    // Linux glibc
                "/usr/lib/libSystem.dylib" // OSX POSIX
        };
        IntPtr libcLibrary = IntPtr.Zero;
        foreach (string libcName in libcMapping)
        {
            if (DynDll.TryOpenLibrary(libcName, out libcLibrary))
                break;
        }
        if (libcLibrary != IntPtr.Zero)
        {
            dup = (dupDelegate)Marshal.GetDelegateForFunctionPointer(DynDll.GetExport(libcLibrary, "dup"), typeof(dupDelegate));
            fdopen = (fdopenDelegate)Marshal.GetDelegateForFunctionPointer(DynDll.GetExport(libcLibrary, "fdopen"), typeof(fdopenDelegate));
            fread = (freadDelegate)Marshal.GetDelegateForFunctionPointer(DynDll.GetExport(libcLibrary, "fread"), typeof(freadDelegate));
            fwrite = (fwriteDelegate)Marshal.GetDelegateForFunctionPointer(DynDll.GetExport(libcLibrary, "fwrite"), typeof(fwriteDelegate));
            fclose = (fcloseDelegate)Marshal.GetDelegateForFunctionPointer(DynDll.GetExport(libcLibrary, "fclose"), typeof(fcloseDelegate));
            fflush = (fflushDelegate)Marshal.GetDelegateForFunctionPointer(DynDll.GetExport(libcLibrary, "fflush"), typeof(fflushDelegate));
            isatty = (isattyDelegate)Marshal.GetDelegateForFunctionPointer(DynDll.GetExport(libcLibrary, "isatty"), typeof(isattyDelegate));
        }
        
    }

    public static Stream CreateDuplicateStream(int fileDescriptor)
    {
        var newFd = dup(fileDescriptor);

        return new UnixStream(newFd, FileAccess.Write);
    }
}
