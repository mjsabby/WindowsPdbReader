// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace WindowsPdbReader
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct DbiDbgHdr
    {
        public ushort snFPO;                 // 0..1
        public ushort snException;           // 2..3 (deprecated)
        public ushort snFixup;               // 4..5
        public ushort snOmapToSrc;           // 6..7
        public ushort snOmapFromSrc;         // 8..9
        public ushort snSectionHdr;          // 10..11
        public ushort snTokenRidMap;         // 12..13
        public ushort snXdata;               // 14..15
        public ushort snPdata;               // 16..17
        public ushort snNewFPO;              // 18..19
        public ushort snSectionHdrOrig;      // 20..21
    }
}
