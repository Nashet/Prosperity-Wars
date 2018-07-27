using System.Collections.Generic;

namespace Nashet.Utils
{
    public class FixedSizeQueue<T> : Queue<T>
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
                Dequeue();
            base.Enqueue(t);
        }

        public void EnqueueEmpty()
        {
            Enqueue(emptyElement);
        }

        public void Add(FixedSizeQueue<T> populationChanges)
        {
            foreach (var item in populationChanges)
            {
                Enqueue(item);
            }
        }
    }
}