// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WindowsPdbReader
{
    internal sealed class MsfDirectory
    {
        private readonly List<DataStream> streams;

        // @@BPerfIgnoreAllocation@@ -- pages (not poolable due to MemoryMarshal), stream (meh), List<DataStream> lifetime extends method, dsPages lifetime on DataStream, Lifetime outside of scope for DataStream objects
        public MsfDirectory(PageAwarePdbReader reader, int pageSize, int directorySize, int[] directoryRoot)
        {
            int numPages = reader.PagesFromSize(directorySize);

            var pages = new int[numPages];
            var buf = MemoryMarshal.Cast<int, byte>(pages);

            int offset = 0;
            int pagesPerPage = pageSize / 4;
            int pagesToGo = numPages;
            for (int i = 0; i < directoryRoot.Length; ++i)
            {
                int pagesInThisPage = pagesToGo <= pagesPerPage ? pagesToGo : pagesPerPage;
                reader.Seek(directoryRoot[i], 0);
                reader.Stream.Read(buf.Slice(offset, pagesInThisPage * 4));
                pagesToGo -= pagesInThisPage;
            }

            byte[] buffer = default;
            int[] sizes = default;
            try
            {
                buffer = ArrayPool<byte>.Shared.Rent(directorySize);
                var stream = new DataStream(directorySize, pages);
                stream.Read(reader, buffer);

                offset = 0;
                int count = InterpretInt32(buffer, ref offset);

                try
                {
                    // 4..n
                    sizes = ArrayPool<int>.Shared.Rent(count);

                    for (int i = 0; i < count; ++i)
                    {
                        sizes[i] = InterpretInt32(buffer, ref offset);
                    }

                    streams = new List<DataStream>(count);

                    for (int i = 0; i < count; ++i)
                    {
                        if (sizes[i] <= 0)
                        {
                            streams.Add(new DataStream());
                        }
                        else
                        {
                            int dsPagesCount = reader.PagesFromSize(sizes[i]);
                            if (dsPagesCount > 0)
                            {
                                var dsPages = new int[dsPagesCount];
                                for (int j = 0; j < dsPagesCount; ++j)
                                {
                                    dsPages[j] = InterpretInt32(buffer, ref offset);
                                }

                                streams.Add(new DataStream(sizes[i], dsPages));
                            }
                            else
                            {
                                streams.Add(new DataStream(sizes[i], null));
                            }
                        }
                    }
                }
                finally
                {
                    if (sizes != null)
                    {
                        ArrayPool<int>.Shared.Return(sizes);
                    }
                }
            }
            finally
            {
                if (buffer != null)
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }

        public List<DataStream> Streams => this.streams;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int InterpretInt32(Span<byte> buffer, ref int offset)
        {
            var retval = (int)((buffer[offset + 0] & 0xFF) |
                         (buffer[offset + 1] << 8) |
                         (buffer[offset + 2] << 16) |
                         (buffer[offset + 3] << 24));
            offset += 4;
            return retval;
        }
    }
}