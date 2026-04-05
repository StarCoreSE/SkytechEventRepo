using System.Collections.Generic;

namespace Collections
{
    public class CleanedSet<TItem> : HashSet<TItem> where TItem : IClosable
    {
        private List<TItem> _closed = new List<TItem>();

        public bool RunCleanup()
        {
            foreach (TItem item in this)
            {
                if (item.IsClosed)
                {
                    _closed.Add(item);
                }
            }

            foreach (var item in _closed)
            {
                Remove(item);
            }

            bool anyRemoved = _closed.Count > 0;
            _closed.Clear();
            return anyRemoved;
        }
    }

    public class CleanedList<TItem> : List<TItem> where TItem : IClosable
    {
        private List<TItem> _closed = new List<TItem>();

        public bool RunCleanup()
        {
            foreach (TItem item in this)
            {
                if (item.IsClosed)
                {
                    _closed.Add(item);
                }
            }

            foreach (var item in _closed)
            {
                Remove(item);
            }

            bool anyRemoved = _closed.Count > 0;
            _closed.Clear();
            return anyRemoved;
        }
    }

    public interface IClosable
    {
        bool IsClosed { get; }
    }
}
