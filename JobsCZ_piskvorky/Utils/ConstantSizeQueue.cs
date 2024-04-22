using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsCZ_piskvorky.Utils
{
    public class ConstantSizeQueue<T>
    {
        private readonly Queue<T> queue = new Queue<T>();
        private int size = 0;
        public int Size
        {
            get
            {
                return size;
            }

            set
            {
                size = value;

                while (queue.Count > size)
                    queue.Dequeue();
            }
        }

        public void Enqueu(T item)
        {
            queue.Enqueue(item);
        }

        public void Clear()
        {
            queue.Clear();
        }

        public T this[int index]
        {
            get
            {
                return queue.ElementAt(index);
            }
        }

        public List<T> ToList()
        {
            return queue.ToList();
        }
    }
}
