// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WindowsPdbReader
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class MsfDirectory
    {
        private readonly DataStream[] streams;

        public MsfDirectory(PdbReader reader, int pageSize, int directorySize, int[] directoryRoot)
        {
            var numPages = reader.PagesFromSize(directorySize);

            var pages = new int[numPages];
            Span<byte> buf = MemoryMarshal.Cast<int, byte>(pages);

            var offset = 0;
            var pagesPerPage = pageSize / 4;
            var pagesToGo = numPages;

            foreach (var t in directoryRoot)
            {
                var pagesInThisPage = pagesToGo <= pagesPerPage ? pagesToGo : pagesPerPage;
                reader.Seek(t, 0);
                reader.Stream.Read(buf.Slice(offset, pagesInThisPage * 4));
                pagesToGo -= pagesInThisPage;
            }

            using var buffer = new SafeArrayPool<byte>(directorySize);
            var stream = new DataStream(directorySize, pages);
            stream.Read(reader, buffer.Array);

            offset = 0;
            var count = InterpretInt32(buffer.Array, ref offset);

            using var sizesPool = new SafeArrayPool<int>(count);
            int[] sizes = sizesPool.Array;

            for (var i = 0; i < count; ++i)
            {
                sizes[i] = InterpretInt32(buffer.Array, ref offset);
            }

            this.streams = new DataStream[count];

            for (var i = 0; i < count; ++i)
            {
                if (sizes[i] <= 0)
                {
                    this.streams[i] = new DataStream();
                }
                else
                {
                    var dsPagesCount = reader.PagesFromSize(sizes[i]);
                    if (dsPagesCount > 0)
                    {
                        var dsPages = new int[dsPagesCount];

                        for (var j = 0; j < dsPagesCount; ++j)
                        {
                            dsPages[j] = InterpretInt32(buffer.Array, ref offset);
                        }

                        this.streams[i] = new DataStream(sizes[i], dsPages);
                    }
                    else
                    {
                        this.streams[i] = new DataStream(sizes[i], Array.Empty<int>());
                    }
                }
            }
        }

        public DataStream[] Streams => this.streams;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int InterpretInt32(Span<byte> buffer, ref int offset)
        {
            var retval = (buffer[offset + 0] & 0xFF) |
                         (buffer[offset + 1] << 8) |
                         (buffer[offset + 2] << 16) |
                         (buffer[offset + 3] << 24);
            offset += 4;
            return retval;
        }
    }
}
