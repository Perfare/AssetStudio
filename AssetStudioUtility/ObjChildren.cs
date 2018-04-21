using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace AssetStudio
{
    public interface IObjChild
    {
        dynamic Parent { get; set; }
    }

    public abstract class ObjChildren<T> : IEnumerable<T> where T : IObjChild
    {
        protected List<T> children;

        public T this[int i] => children[i];

        public int Count => children.Count;

        public void InitChildren(int count)
        {
            children = new List<T>(count);
        }

        public void AddChild(T obj)
        {
            children.Add(obj);
            obj.Parent = this;
        }

        public void InsertChild(int i, T obj)
        {
            children.Insert(i, obj);
            obj.Parent = this;
        }

        public void RemoveChild(T obj)
        {
            obj.Parent = null;
            children.Remove(obj);
        }

        public void RemoveChild(int i)
        {
            children[i].Parent = null;
            children.RemoveAt(i);
        }

        public int IndexOf(T obj)
        {
            return children.IndexOf(obj);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
