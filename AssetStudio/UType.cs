using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class UType : IDictionary<string, object>
    {
        private List<string> keys;
        private IDictionary<string, object> values;

        public UType()
        {
            keys = new List<string>();
            values = new Dictionary<string, object>();
        }

        public object this[string key]
        {
            get
            {
                if (!values.ContainsKey(key))
                {
                    return null;
                }
                return values[key];
            }
            set
            {
                if (!values.ContainsKey(key))
                {
                    keys.Add(key);
                }
                values[key] = value;
            }
        }

        public ICollection<string> Keys => keys;

        public ICollection<object> Values => values.Values;

        public int Count => keys.Count;

        public bool IsReadOnly => false;

        public void Add(string key, object value)
        {
            keys.Add(key);
            values.Add(key, value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            keys.Add(item.Key);
            values.Add(item);
        }

        public void Clear()
        {
            keys.Clear();
            values.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return values.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return values.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            values.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        public bool Remove(string key)
        {
            keys.Remove(key);
            return values.Remove(key);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            keys.Remove(item.Key);
            return values.Remove(item);
        }

        public bool TryGetValue(string key, out object value)
        {
            return values.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }
    }
}
