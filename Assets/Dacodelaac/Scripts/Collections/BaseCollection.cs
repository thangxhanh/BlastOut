using System;
using System.Collections;
using System.Collections.Generic;
using Dacodelaac.Core;
using UnityEngine;

namespace Dacodelaac.Collections
{
    public class BaseCollection<T> : BaseSO, IEnumerable<T>, ISerializationCallbackReceiver
    {
        [SerializeField] bool clearOnPlay = true;
        [SerializeField] List<T> list = new List<T>();

        public T this[int index]
        {
            get => list[index];
            set => list[index] = value;
        }

        public IList<T> List => list;

        public Type Type => typeof(T);
        public int Count => List.Count;

        public void Add(T obj)
        {
            list.Add(obj);
        }

        public void Remove(T obj)
        {
            if (Contains(obj))
            {
                list.Remove(obj);
            }
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(T value)
        {
            return list.Contains(value);
        }

        public int IndexOf(T value)
        {
            return list.IndexOf(value);
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        public void Insert(int index, T value)
        {
            list.Insert(index, value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public override string ToString()
        {
            return "Collection<" + typeof(T) + ">(" + Count + ")";
        }

        public T[] ToArray()
        {
            return list.ToArray();
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            if (clearOnPlay)
            {
                Clear();
            }
        }
    }
}