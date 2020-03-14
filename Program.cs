// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WindowsPdbReader
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args?.Length != 3)
            {
                Console.WriteLine("WindowsPdbReader [PdbFile] [MethodToken] [ILOffset]");
                return;
            }

            using var fs = new FileStream(args[0], FileMode.Open, FileAccess.Read);
            if (GetSourceLineInfo(fs, uint.Parse(args[1], CultureInfo.InvariantCulture), uint.Parse(args[2], CultureInfo.InvariantCulture), out var age, out Guid guid, out var sourceFile, out var sourceLine, out var sourceColumn))
            {
                Console.WriteLine($"File: {sourceFile}, Line: {sourceLine}, Column: {sourceColumn}");
            }
        }

        private static bool GetSourceLineInfo(Stream fs, uint token, uint iloffset, out int age, out Guid guid, out string sourceFile, out int sourceLine, out int sourceColumn)
        {
            if (ReadPdbHeader(fs, out var pageSize, out var freePageMap, out var pagesUsed, out var directorySize, out var zero))
            {
                var directoryPages = (((directorySize + pageSize - 1) / pageSize * 4) + pageSize - 1) / pageSize;

                var directoryRoot = new int[directoryPages];
                fs.Read(MemoryMarshal.Cast<int, byte>(directoryRoot));

                var pageAwarePdbReader = new PdbReader(fs, pageSize);
                var msf = new MsfDirectory(pageAwarePdbReader, pageSize, directorySize, directoryRoot);

                DataStream nameIndexStream = msf.Streams[1];
                var nameIndexStreamData = new byte[nameIndexStream.Length];
                nameIndexStream.Read(pageAwarePdbReader, nameIndexStreamData);

                IReadOnlyDictionary<string, int> nameIndex = LoadNameIndex(nameIndexStreamData, out age, out guid);
                if (nameIndex.TryGetValue("/NAMES", out var namesStreamIndex))
                {
                    DataStream namesStream = msf.Streams[namesStreamIndex];
                    var namesStreamData = new byte[namesStream.Length];
                    namesStream.Read(pageAwarePdbReader, namesStreamData);

                    IReadOnlyDictionary<int, string> names = LoadNameStream(namesStreamData);

                    DataStream dbiStream = msf.Streams[3];
                    var dbiStreamData = new byte[dbiStream.Length];
                    dbiStream.Read(pageAwarePdbReader, dbiStreamData);

                    IReadOnlyList<DbiModuleInfo> modules = LoadModules(dbiStreamData);
                    GetSourceLineInfoInner(modules, msf, pageAwarePdbReader, names, token, iloffset, out sourceFile, out sourceLine, out sourceColumn);

                    return true;
                }
            }

            age = 0;
            guid = Guid.Empty;
            sourceFile = string.Empty;
            sourceLine = 0;
            sourceColumn = 0;

            return false;
        }

        private static void GetSourceLineInfoInner(IReadOnlyList<DbiModuleInfo> modules, MsfDirectory dir, PdbReader reader, IReadOnlyDictionary<int, string> names, uint token, uint iloffset, out string sourceFile, out int sourceLine, out int sourceColumn)
        {
            sourceFile = string.Empty;
            sourceLine = 0;
            sourceColumn = 0;

            foreach (DbiModuleInfo module in modules)
            {
                if (module.stream > 0)
                {
                    DataStream stream = dir.Streams[module.stream];

                    using var bitsPool = new SafeArrayPool<byte>(stream.Length);
                    byte[] bits = bitsPool.Array;
                    stream.Read(reader, bits);

                    if (TryGetManProcSym(bits, in module, token, out ManProcSym sym))
                    {
                        GetLineNumberInformation(bits, in module, sym.off + iloffset, names, out sourceFile, out sourceLine, out sourceColumn);
                    }
                }
            }
        }

        private static bool TryGetManProcSym(ReadOnlySpan<byte> bits, in DbiModuleInfo module, uint token, out ManProcSym proc)
        {
            var offset = 0;
            var sig = InterpretInt32(bits, ref offset);

            if (sig != 4)
            {
                throw new Exception($"Invalid signature. (sig={sig})");
            }

            while (offset < module.cbSyms)
            {
                Console.Write($"({offset:x8}) ");

                var siz = InterpretUInt16(bits, ref offset);
                var stop = offset + siz;
                var rec = InterpretUInt16(bits, ref offset);

                Console.WriteLine($"{(SYM)rec}");

                switch ((SYM)rec)
                {
                    case SYM.S_GMANPROC:
                    case SYM.S_LMANPROC:
                    {
                        proc = InterpretStruct<ManProcSym>(in bits, ref offset); // copy

                        if (proc.token == token)
                        {
                            return true;
                        }

                        SkipCString(bits, ref offset);

                        break;
                    }
                }

                offset = stop;
            }

            proc = default;
            return false;
        }

        private static void GetLineNumberInformation(in ReadOnlySpan<byte> streamData, in DbiModuleInfo module, uint functionOffset, IReadOnlyDictionary<int, string> names, out string sourceFile, out int sourceLine, out int sourceColumn)
        {
            sourceFile = string.Empty;
            sourceLine = 0;
            sourceColumn = 0;

            var offset = module.cbSyms + module.cbOldLines;
            var limit = offset + module.cbLines;

            while (offset < limit)
            {
                var sig = InterpretInt32(streamData, ref offset);
                var siz = InterpretInt32(streamData, ref offset);
                var endSym = offset + siz;

                switch ((DEBUG_S_SUBSECTION)sig)
                {
                    case DEBUG_S_SUBSECTION.LINES:
                    {
                        ref readonly CV_LineSection sec = ref InterpretStruct<CV_LineSection>(in streamData, ref offset);

                        // BUG: Needs to match the bestPointSoFar model
                        if (functionOffset >= sec.off && functionOffset <= sec.off + sec.cod)
                        {
                            ref readonly CV_SourceFile file = ref InterpretStruct<CV_SourceFile>(in streamData, ref offset);

                            var plin = offset;
                            var pcol = offset + (8 * (int)file.count);

                            for (var i = 0; i < file.count; i++)
                            {
                                CV_Line line;
                                CV_Column column = default;

                                offset = plin + (8 * i);
                                line.offset = InterpretUInt32(streamData, ref offset);
                                line.flags = InterpretUInt32(streamData, ref offset);

                                var lineBegin = line.flags & (uint)CV_Line_Flags.linenumStart;

                                if ((sec.flags & 1) != 0)
                                {
                                    offset = pcol + (4 * i);
                                    column.offColumnStart = InterpretUInt16(streamData, ref offset);
                                    column.offColumnEnd = InterpretUInt16(streamData, ref offset);
                                }

                                sourceLine = (int)lineBegin;
                                sourceColumn = column.offColumnStart;
                            }
                        }

                        break;
                    }

                    case DEBUG_S_SUBSECTION.FILECHKSMS:
                    {
                        while (offset < limit)
                        {
                            ref readonly CV_FileCheckSum chk = ref InterpretStruct<CV_FileCheckSum>(in streamData, ref offset);
                            offset += chk.len;
                            sourceFile = names[(int)chk.name]; // BUG: Is this really correct? Maybe it should be file.index?
                            Alignment(4, ref offset);
                        }

                        break;
                    }
                }

                offset = endSym;
            }
        }

        private static IReadOnlyList<DbiModuleInfo> LoadModules(ReadOnlySpan<byte> bits)
        {
            var offset = 0;

            ref readonly DbiHeader dbiHeader = ref InterpretStruct<DbiHeader>(in bits, ref offset);

            var modules = new List<DbiModuleInfo>();

            var end = offset + dbiHeader.gpmodiSize;
            while (offset < end)
            {
                ref readonly DbiModuleInfo moduleInfo = ref InterpretStruct<DbiModuleInfo>(in bits, ref offset);
                SkipCString(bits, ref offset); // moduleName
                SkipCString(bits, ref offset); // objectName
                Alignment(4, ref offset);
                modules.Add(moduleInfo);
            }

            if (offset != end)
            {
                throw new Exception($"Error reading DBI stream, pos={offset} != {end}");
            }

            // Skip the Section Contribution substream.
            offset += dbiHeader.secconSize;

            // Skip the Section Map substream.
            offset += dbiHeader.secmapSize;

            // Skip the File Info substream.
            offset += dbiHeader.filinfSize;

            // Skip the TSM substream.
            offset += dbiHeader.tsmapSize;

            // Skip the EC substream.
            offset += dbiHeader.ecinfoSize;

            // Read the optional header.
            end = offset + dbiHeader.dbghdrSize;
            if (dbiHeader.dbghdrSize > 0)
            {
                ref readonly DbiDbgHdr header = ref InterpretStruct<DbiDbgHdr>(in bits, ref offset);
            }

            offset = end;

            return modules;
        }

        private static IReadOnlyDictionary<int, string> LoadNameStream(ReadOnlySpan<byte> bits)
        {
            var offset = 0;

            var sig = InterpretUInt32(bits, ref offset);
            var ver = InterpretInt32(bits, ref offset);

            if (sig != 0xeffeeffe || ver != 1)
            {
                throw new Exception($"Unsupported Name Stream version.(sig={sig:x8}, ver={ver})");
            }

            var buf = InterpretInt32(bits, ref offset);
            var beg = offset;
            var nxt = offset + buf;
            offset = nxt;

            // Read hash table.
            var siz = InterpretInt32(bits, ref offset);
            var result = new Dictionary<int, string>(siz);

            for (var i = 0; i < siz; i++)
            {
                var ni = InterpretInt32(bits, ref offset);

                if (ni != 0)
                {
                    var saved = offset;
                    offset = beg + ni;
                    var name = ReadCString(bits, ref offset);
                    offset = saved;

                    result.Add(ni, name);
                }
            }

            return result;
        }

        private static IReadOnlyDictionary<string, int> LoadNameIndex(ReadOnlySpan<byte> bits, out int age, out Guid guid)
        {
            var offset = 0;

            var ver = InterpretInt32(bits, ref offset);
            var sig = InterpretInt32(bits, ref offset);

            age = InterpretInt32(bits, ref offset);
            InterpretGuid(bits, out guid, ref offset);

            var buf = InterpretInt32(bits, ref offset);

            var beg = offset;
            var nxt = offset + buf;

            offset = nxt;

            var cnt = InterpretInt32(bits, ref offset);
            var max = InterpretInt32(bits, ref offset);

            var present = new uint[InterpretInt32(bits, ref offset)];
            for (var i = 0; i < present.Length; ++i)
            {
                present[i] = InterpretUInt32(bits, ref offset);
            }

            var deleted = new uint[InterpretInt32(bits, ref offset)];
            for (var i = 0; i < deleted.Length; ++i)
            {
                deleted[i] = InterpretUInt32(bits, ref offset);
            }

            var result = new Dictionary<string, int>(max, StringComparer.OrdinalIgnoreCase);
            var j = 0;
            for (var i = 0; i < max; i++)
            {
                if (IsSet(i, present))
                {
                    var ns = InterpretInt32(bits, ref offset);
                    var ni = InterpretInt32(bits, ref offset);

                    var saved = offset;
                    offset = beg + ns;
                    var name = ReadCString(bits, ref offset);
                    offset = saved;

                    result.Add(name, ni);
                    j++;
                }
            }

            if (j != cnt)
            {
                throw new Exception($"Count mismatch. ({j} != {cnt})");
            }

            return result;
        }

        private static bool IsSet(int index, uint[] words)
        {
            var word = index / 32;

            if (word >= words.Length)
            {
                return false;
            }

            return (words[word] & (1u << (index % 32))) != 0;
        }

        private static bool ReadPdbHeader(Stream stream, out int pageSize, out int freePageMap, out int pagesUsed, out int directorySize, out int zero)
        {
            ReadOnlySpan<byte> pdbMagic = new byte[]
            {
                0x4D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, // "Microsof"
                0x74, 0x20, 0x43, 0x2F, 0x43, 0x2B, 0x2B, 0x20, // "t C/C++ "
                0x4D, 0x53, 0x46, 0x20, 0x37, 0x2E, 0x30, 0x30, // "MSF 7.00"
                0x0D, 0x0A, 0x1A, 0x44, 0x53, 0x00, 0x00, 0x00,  // "^^^DS^^^"
            };

            Span<byte> fileMagic = stackalloc byte[32];
            stream.Read(fileMagic);

            if (fileMagic.SequenceEqual(pdbMagic))
            {
                Span<byte> basicInfo = stackalloc byte[20];
                stream.Read(basicInfo);

                pageSize = InterpretInt32(basicInfo, 0);
                freePageMap = InterpretInt32(basicInfo, 4);
                pagesUsed = InterpretInt32(basicInfo, 8);
                directorySize = InterpretInt32(basicInfo, 12);
                zero = InterpretInt32(basicInfo, 16);

                return true;
            }

            pageSize = 0;
            freePageMap = 0;
            pagesUsed = 0;
            directorySize = 0;
            zero = 0;

            return false;
        }

        private static void Alignment(int alignment, ref int offset)
        {
            while (offset % alignment != 0)
            {
                offset++;
            }
        }

        private static void SkipCString(ReadOnlySpan<byte> buffer, ref int offset)
        {
            var len = 0;

            while (offset + len < buffer.Length && buffer[offset + len] != 0)
            {
                len++;
            }

            offset += len + 1;
        }

        private static string ReadCString(ReadOnlySpan<byte> buffer, ref int offset)
        {
            var len = 0;

            while (offset + len < buffer.Length && buffer[offset + len] != 0)
            {
                len++;
            }

            var value = Encoding.UTF8.GetString(buffer.Slice(offset, len));

            offset += len + 1;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref readonly T InterpretStruct<T>(in ReadOnlySpan<byte> data, ref int offset)
            where T : unmanaged
        {
            ReadOnlySpan<byte> slice = data.Slice(offset);
            offset += Marshal.SizeOf<T>();
            return ref MemoryMarshal.AsRef<T>(slice);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int InterpretInt32(ReadOnlySpan<byte> buffer, int offset)
        {
            return (buffer[offset + 0] & 0xFF) | (buffer[offset + 1] << 8) | (buffer[offset + 2] << 16) | (buffer[offset + 3] << 24);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint InterpretUInt32(ReadOnlySpan<byte> buffer, ref int offset)
        {
            var retval = (uint)((buffer[offset + 0] & 0xFF) | (buffer[offset + 1] << 8) | (buffer[offset + 2] << 16) | (buffer[offset + 3] << 24));
            offset += 4;
            return retval;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int InterpretInt32(ReadOnlySpan<byte> buffer, ref int offset)
        {
            var retval = (buffer[offset + 0] & 0xFF) | (buffer[offset + 1] << 8) | (buffer[offset + 2] << 16) | (buffer[offset + 3] << 24);
            offset += 4;
            return retval;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InterpretGuid(ReadOnlySpan<byte> buffer, out Guid retval, ref int offset)
        {
            retval = new Guid(buffer.Slice(offset, 16));
            offset += 16;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ushort InterpretUInt16(ReadOnlySpan<byte> buffer, ref int offset)
        {
            var retval = (ushort)((buffer[offset + 0] & 0xFF) | (buffer[offset + 1] << 8));
            offset += 2;
            return retval;
        }
    }
}
