// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.Collections.Generic;

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    public class MemoryPool : IMemoryPool
    {
        internal static readonly byte[] EmptyArray = new byte[0];

        readonly ArrayPool<byte> _pool1 = ArrayPool<byte>.Create(1024, 256);
        readonly ArrayPool<byte> _pool2 = ArrayPool<byte>.Create(2048, 64);
        readonly ArrayPool<char> _pool3 = ArrayPool<char>.Create(128, 256);

        public byte[] AllocByte(int minimumSize)
        {
            if (minimumSize == 0)
            {
                return EmptyArray;
            }
            if (minimumSize <= 1024)
            {
                return _pool1.Rent(1024);
            }
            if (minimumSize <= 2048)
            {
                return _pool2.Rent(2048);
            }
            return new byte[minimumSize];
        }

        public void FreeByte(byte[] memory)
        {
            if (memory == null)
            {
                return;
            }
            switch (memory.Length)
            {
                case 1024:
                    _pool1.Return(memory);
                    break;
                case 2048:
                    _pool2.Return(memory);
                    break;
            }
        }

        public char[] AllocChar(int minimumSize)
        {
            if (minimumSize == 0)
            {
                return new char[0];
            }
            if (minimumSize <= 128)
            {
                return _pool3.Rent(128);
            }
            return new char[minimumSize];
        }

        public void FreeChar(char[] memory)
        {
            if (memory == null)
            {
                return;
            }
            switch (memory.Length)
            {
                case 128:
                    _pool3.Return(memory);
                    break;
            }
        }

        public ArraySegment<byte> AllocSegment(int minimumSize)
        {
            return new ArraySegment<byte>(AllocByte(minimumSize));
        }

        public void FreeSegment(ArraySegment<byte> segment)
        {
            FreeByte(segment.Array);
        }
    }
}