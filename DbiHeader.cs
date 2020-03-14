// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WindowsPdbReader
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct DbiHeader
    {
        public int sig;                        // 0..3
        public int ver;                        // 4..7
        public int age;                        // 8..11
        public short gssymStream;              // 12..13
        public ushort vers;                    // 14..15
        public short pssymStream;              // 16..17
        public ushort pdbver;                  // 18..19
        public short symrecStream;             // 20..21
        public ushort pdbver2;                 // 22..23
        public int gpmodiSize;                 // 24..27
        public int secconSize;                 // 28..31
        public int secmapSize;                 // 32..35
        public int filinfSize;                 // 36..39
        public int tsmapSize;                  // 40..43
        public int mfcIndex;                   // 44..47
        public int dbghdrSize;                 // 48..51
        public int ecinfoSize;                 // 52..55
        public ushort flags;                   // 56..57
        public ushort machine;                 // 58..59
        public int reserved;                   // 60..63
    }
}
