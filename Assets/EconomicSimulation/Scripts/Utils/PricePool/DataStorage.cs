using Nashet.ValueSpace;

namespace Nashet.Utils
{
	public class DataStorage<IDTYPE>
    {
        private static int length = 40;

        //todo use LinkedList<T> instead of queue?
        public LimitedQueue<Value> data;

        private IDTYPE ID;

        public DataStorage(IDTYPE inn)
        {
            data = new LimitedQueue<Value>(length);
            ID = inn;
        }

        public void addData(Value indata)
        {
            data.Enqueue(new Value(indata.get()));
        }
    }
}