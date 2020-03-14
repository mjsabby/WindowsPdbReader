// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WindowsPdbReader
{
    using System.Buffers;

    internal ref struct SafeArrayPool<T>
    {
        private readonly T[] buf;

        public SafeArrayPool(int size)
        {
            this.buf = ArrayPool<T>.Shared.Rent(size);
        }

        public T[] Array => this.buf;

        public void Dispose()
        {
            ArrayPool<T>.Shared.Return(this.buf);
        }
    }
}
