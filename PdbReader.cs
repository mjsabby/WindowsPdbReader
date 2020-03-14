// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WindowsPdbReader
{
    using System.IO;
    using System.Runtime.CompilerServices;

    internal sealed class PdbReader
    {
        private readonly Stream stream;

        private readonly int pageSize;

        public PdbReader(Stream stream, int pageSize)
        {
            this.stream = stream;
            this.pageSize = pageSize;
        }

        public int PageSize => this.pageSize;

        public Stream Stream => this.stream;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Seek(int page, int offset)
        {
            this.stream.Seek((page * this.pageSize) + offset, SeekOrigin.Begin);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(byte[] bytes, int offset, int count)
        {
            this.stream.Read(bytes, offset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int PagesFromSize(int size)
        {
            return (size + this.pageSize - 1) / this.pageSize;
        }
    }
}
