// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WindowsPdbReader
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct DbiSecCon
    {
        public short section;                    // 0..1
        public short pad1;                       // 2..3
        public int offset;                       // 4..7
        public int size;                         // 8..11
        public uint flags;                       // 12..15
        public short module;                     // 16..17
        public short pad2;                       // 18..19
        public uint dataCrc;                     // 20..23
        public uint relocCrc;                    // 24..27
    }
}
