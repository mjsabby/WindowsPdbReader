// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WindowsPdbReader
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct DbiModuleInfo
    {
        internal int opened;                 //  0..3
        internal DbiSecCon section;          //  4..31
        internal ushort flags;               // 32..33
        internal short stream;               // 34..35
        internal int cbSyms;                 // 36..39
        internal int cbOldLines;             // 40..43
        internal int cbLines;                // 44..57
        internal short files;                // 48..49
        internal short pad1;                 // 50..51
        internal uint offsets;               // 52..55
        internal int niSource;               // 56..59
        internal int niCompiler;             // 60..63
    }
}
