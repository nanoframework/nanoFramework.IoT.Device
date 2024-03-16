// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BlueNrg2.Utils
{
    internal class List
    {
        private Node _head;

        internal bool Empty()
        {
            return _head is null;
        }

        internal int Length()
        {
            if (_head is null) return 0;
            var result = _head;
            int i;
            for (i = 1; result.Next != null; i++, result = result.Next)
            {
            }

            return i;
        }

        internal void MoveTo(List destination)
        {
            object temp;

            while (!Empty())
            {
                temp = RemoveTail();
                destination.InsertHead(temp);
            }
        }

        internal object RemoveTail()
        {
            var result = _head;
            while (result.Next.Next != null) result = result.Next;

            var temp = result;
            result = result.Next;
            temp.Next = null;

            return result;
        }

        internal object RemoveHead()
        {
            if (_head is null) return null;

            var result = _head;
            _head = _head.Next;

            return result.ObjectData;
        }

        internal void InsertHead(object o)
        {
            var temp = _head;
            _head = new Node(o) { Next = temp };
        }

        internal void InsertTail(object o)
        {
            if (_head is null)
            {
                _head = new Node(o);
                return;
            }

            GetLastNode().Next = new Node(0);
        }

        // WARNING: Do not use without checking if _head is null first
        private Node GetLastNode()
        {
            var result = _head!;
            while (result.Next != null) result = result.Next;

            return result;
        }

        internal class Node
        {
            internal readonly object ObjectData;
            internal Node Next;

            public Node(object o)
            {
                ObjectData = o;
                Next = null;
            }
        }
    }
}
