﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;

internal static partial class Interop
{
    internal static partial class Kernel32
    {
        [SuppressGCTransition]
        [LibraryImport(Libs.Kernel32)]
        internal static partial IntPtr GetStdHandle(int nStdHandle);  // param is NOT a handle, but it returns one!
    }
}