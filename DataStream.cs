// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;

namespace WindowsPdbReader
{
    internal sealed class DataStream
    {
        private readonly int contentSize;

        private readonly int[] pages;

        public DataStream()
        {
        }

        public DataStream(int contentSize, int[] pages)
        {
            this.contentSize = contentSize;
            this.pages = pages;
        }

        public int Length => this.contentSize;

        public void Read(PageAwarePdbReader reader, byte[] buffer)
        {
            if (buffer.Length < this.contentSize)
            {
                ThrowInsufficientBufferSizeException();
            }

            this.Read(reader, 0, buffer, 0, contentSize);
        }

        private void Read(PageAwarePdbReader reader, int position, byte[] bytes, int offset, int data)
        {
            if (position + data > this.contentSize)
            {
                ThrowDataStreamEndOfFileException(position, data);
            }

            if (position == this.contentSize)
            {
                return;
            }

            int left = data;
            int page = position / reader.PageSize;
            int rema = position % reader.PageSize;

            // First get remained of first page.
            if (rema != 0)
            {
                int todo = reader.PageSize - rema;
                if (todo > left)
                {
                    todo = left;
                }

                reader.Seek(this.pages[page], rema);
                reader.Read(bytes, offset, todo);

                offset += todo;
                left -= todo;
                page++;
            }

            // Now get the remaining pages.
            while (left > 0)
            {
                int todo = reader.PageSize;
                if (todo > left)
                {
                    todo = left;
                }

                reader.Seek(this.pages[page], 0);
                reader.Read(bytes, offset, todo);

                offset += todo;
                left -= todo;
                page++;
            }
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
    }
}