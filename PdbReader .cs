// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Runtime.CompilerServices;

namespace WindowsPdbReader
{
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
            this.stream.Seek(page * this.pageSize + offset, SeekOrigin.Begin);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(byte[] bytes, int offset, int count)
        {
            this.stream.Read(bytes, offset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int PagesFromSize(int size)
        {
            return (size + this.pageSize - 1) / pageSize;
        }
    }
}