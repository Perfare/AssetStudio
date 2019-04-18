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
        private List<object> values;

        public UType()
        {
            keys = new List<string>();
            values = new List<object>();
        }

        private int GetValueIndex(string name)
        {
            for (int i = 0, n = keys.Count; i < n; i++)
            {
                if (string.Equals(keys[i], name, StringComparison.Ordinal))
                {
                    return i;
                }
            }
            return -1;
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            var index = GetValueIndex(key);
            if (index != -1)
            {
                value = (T)values[index];
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        public object this[string key]
        {
            get
            {
                var index = GetValueIndex(key);
                if (index != -1)
                {
                    return values[index];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                var index = GetValueIndex(key);
                if (index == -1)
                {
                    keys.Add(key);
                    values.Add(value);
                }
                else
                {
                    values[index] = value;
                }
            }
        }

        public ICollection<string> Keys => keys;

        public ICollection<object> Values => values;

        public int Count => keys.Count;

        public bool IsReadOnly => false;

        public void Add(string key, object value)
        {
            keys.Add(key);
            values.Add(value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            keys.Add(item.Key);
            values.Add(item.Value);
        }

        public void Clear()
        {
            keys.Clear();
            values.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            return GetValueIndex(key) != -1;
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            for (int i = 0, n = keys.Count; i < n; i++)
            {
                yield return new KeyValuePair<string, object>(keys[i], values[i]);
            }
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out object value)
        {
            var index = GetValueIndex(key);
            if (index != -1)
            {
                value = values[index];
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
