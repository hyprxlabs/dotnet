﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

internal static partial class Interop
{
    internal static partial class Kernel32
    {
#if NET7_0_OR_GREATER
        [LibraryImport(Libs.Kernel32)]
        internal static partial int GetLastError();
#else
        [DllImport(Libs.Kernel32)]
        internal static extern int GetLastError();
#endif
    }
}