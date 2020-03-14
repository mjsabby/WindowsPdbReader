// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WindowsPdbReader
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class DataStream
    {
        private readonly int contentSize;

        private readonly int[] pages;

        public DataStream()
        {
            this.contentSize = 0;
            this.pages = Array.Empty<int>();
        }

        public DataStream(int contentSize, int[] pages)
        {
            this.contentSize = contentSize;
            this.pages = pages;
        }

        public int Length => this.contentSize;

        public void Read(PdbReader reader, byte[] buffer)
        {
            if (buffer.Length < this.contentSize)
            {
                ThrowInsufficientBufferSizeException();
            }

            this.Read(reader, 0, buffer, 0, this.contentSize);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowInsufficientBufferSizeException()
        {
            throw new Exception($"buffer size is smaller than content size");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowDataStreamEndOfFileException(int position, int data)
        {
            throw new Exception($"DataStream can't read off end of stream. (pos={position},siz={data})");
        }

        private void Read(PdbReader reader, int position, byte[] bytes, int offset, int data)
        {
            if (position + data > this.contentSize)
            {
                ThrowDataStreamEndOfFileException(position, data);
            }

            if (position == this.contentSize)
            {
                return;
            }

            var left = data;
            var page = position / reader.PageSize;
            var rema = position % reader.PageSize;

            // First get remained of first page.
            if (rema != 0)
            {
                var todo = reader.PageSize - rema;
                if (todo > left)
                {
                    todo = left;
                }

                reader.Seek(this.pages[page], rema);
                reader.Read(bytes, offset, todo);

                offset += todo;
                left -= todo;
                ++page;
            }

            // Now get the remaining pages.
            while (left > 0)
            {
                var todo = reader.PageSize;
                if (todo > left)
                {
                    todo = left;
                }

                reader.Seek(this.pages[page], 0);
                reader.Read(bytes, offset, todo);

                offset += todo;
                left -= todo;
                ++page;
            }
        }
    }
}
