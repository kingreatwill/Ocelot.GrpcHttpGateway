using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Built.Grpcc.Utils
{
    public class ProducerConsumer<T>
    {
        private readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();

        private readonly Action<T> consumerAction;

        public ProducerConsumer(Action<T> consumerAction)
        {
            this.consumerAction = consumerAction ?? throw new ArgumentNullException("consumerAction");
            Task.Factory.StartNew(this.ConsumeListen);
        }

        //进列
        public void Enqueue(T item)
        {
            queue.Enqueue(item);
        }

        //消费者侦听
        private void ConsumeListen()
        {
            while (true)
            {
                if (this.queue.Count > 0 && this.queue.TryDequeue(out T nextItem))
                {
                    this.consumerAction(nextItem);
                }
                Thread.Sleep(100);
            }
        }
    }
}