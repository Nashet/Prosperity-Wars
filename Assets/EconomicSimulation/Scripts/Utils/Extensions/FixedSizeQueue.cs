using UnityEngine;
using System.Collections.Generic;
using Nashet.EconomicSimulation;
using System;

namespace Nashet.Utils
{
    public class FixedSizeQueue<T> : System.Collections.Generic.Queue<T>
    {
        private readonly int size;
        private readonly T emptyElement;
        public FixedSizeQueue(int size, T emptyElement) : base(size)
        {
            this.emptyElement = emptyElement;
            this.size = size;
        }
        public void Enqueue(T t)
        {
            if (Count == size)
                base.Dequeue();
            base.Enqueue(t);
        }
        public void EnqueueEmpty()
        {
            Enqueue(emptyElement);
        }
        internal void Add(FixedSizeQueue<T> populationChanges)
        {
            foreach (var item in populationChanges)
            {
                Enqueue(item);
            }
        }
    }
}