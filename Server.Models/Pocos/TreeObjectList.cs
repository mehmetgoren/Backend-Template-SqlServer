namespace Server.Models
{
    using System.Collections;
    using System.Collections.Generic;


    public interface ITreeObject
    {
        int Id { get; }
        int? ParentId { get; }
    }

    public interface ITreeObject<T> : ITreeObject
        where T : ITreeObject<T>
    {

        ICollection<T> Children { get; set; }
    }



    public sealed class TreeObjectList<T> : ICollection<T>
        where T : class, ITreeObject<T>
    {
        private sealed class TreeObjectComparer : IEqualityComparer<T>
        {
            internal static readonly TreeObjectComparer Instance = new TreeObjectComparer();

            private TreeObjectComparer()
            {

            }

            public bool Equals(T x, T y)
            {
                return x.Id == y.Id;
            }

            public int GetHashCode(T obj)
            {
                return obj.Id;
            }
        }

        private static T FindRecursive(IEnumerable<T> list, int parentId)
        {
            foreach (var item in list)
            {
                if (item.Id == parentId)
                    return item;

                var temp = FindRecursive(item.Children, parentId);
                if (null != temp)
                    return temp;
            }
            return null;
        }

        private static HashSet<T> CreateList()
        {
            return new HashSet<T>(TreeObjectComparer.Instance);
        }

        private readonly HashSet<T> list = CreateList();

        public void Add(T item)
        {
            if (null != item)
            {
                if (item.Children == null)
                    item.Children = CreateList();

                if (item.ParentId != null)
                {
                    var parent = FindRecursive(this, item.ParentId.Value);
                    if (null != parent)
                    {
                        parent.Children.Add(item);
                        return;
                    }
                }

                this.list.Add(item);
            }
        }
        public void AddRange(IEnumerable<T> en)
        {
            if (null != en)
            {
                foreach (var item in en)
                {
                    this.Add(item);
                }
            }
        }

        public TreeObjectList()
        {

        }

        public TreeObjectList(IEnumerable<T> en)
        {
            this.AddRange(en);
        }

        public void FamilyTheOrphans()
        {
            List<T> forIteration = new List<T>(this.list);
            foreach (var item in forIteration)
            {
                if (item.ParentId > 0)
                {
                    this.Remove(item);//Çünkü her child in sadece bir tane parent i olmak zorunda.
                    this.Add(item);
                }
            }
        }

        public void Clear()
        {
            this.list.Clear();
        }
        public bool Contains(T item)
        {
            return this.list.Contains(item);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            this.list.CopyTo(array, arrayIndex);
        }
        public bool Remove(T item)
        {
            return this.list.Remove(item);
        }
        public int Count => this.list.Count;
        bool ICollection<T>.IsReadOnly => ((ICollection<T>)this.list).IsReadOnly;

        public IEnumerator<T> GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
