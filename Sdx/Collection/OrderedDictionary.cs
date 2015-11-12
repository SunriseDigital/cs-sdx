using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Collection
{
  public class OrderedDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>
  {
    private List<TKey> list;

    private IDictionary<TKey, TValue> dictionary;

    public int Count
    {
      get
      {
        return this.list.Count;
      }
    }

    public bool IsReadOnly
    {
      get
      {
        return this.dictionary.IsReadOnly;
      }
    }

    public ICollection<TKey> Keys
    {
      get
      {
        return this.dictionary.Keys;
      }
    }

    public ICollection<TValue> Values
    {
      get
      {
        return this.dictionary.Values;
      }
    }

    /// <summary>
    /// O(1)
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public TValue this[TKey key]
    {
      get
      {
        return this.dictionary[key];
      }

      set
      {
        if (!this.dictionary.ContainsKey(key))
        {
          this.list.Add(key);
        }
        this.dictionary[key] = value;
      }
    }

    public OrderedDictionary()
    {
      this.list = new List<TKey>();
      this.dictionary = new Dictionary<TKey, TValue>();
    }

    public OrderedDictionary(int capacity):this(capacity, null)
    {

    }

    public OrderedDictionary(int capacity, IEqualityComparer<TKey> comparer)
    {
      this.list = new List<TKey>(capacity);
      this.dictionary = new Dictionary<TKey, TValue>(capacity, comparer);
    }

    public OrderedDictionary(IDictionary<TKey, TValue> dictionary):this()
    {
      foreach(var keyValue in dictionary)
      {
        this[keyValue.Key] = keyValue.Value; 
      }
    }

    /// <summary>
    /// O(1)
    /// </summary>
    /// <param name="item"></param>
    public void Add(KeyValuePair<TKey, TValue> item)
    {
      this[item.Key] = item.Value;
    }

    /// <summary>
    /// O(n) しかもnはcapacity
    /// </summary>
    public void Clear()
    {
      this.list.Clear();
      this.dictionary.Clear();
    }

    /// <summary>
    /// O(1)
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
      return this.dictionary.ContainsKey(item.Key);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      this.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// O(n)
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
      return this.Remove(item.Key);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      foreach (var key in this.list)
      {
        yield return new KeyValuePair<TKey, TValue>(key, this.dictionary[key]);
      }
    }

    public void ForEach(Action<TKey, TValue> action)
    {
      this.list.ForEach(key => {
        action(key, this.dictionary[key]);
      });
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.GetEnumerator();
    }

    /// <summary>
    /// O(1)
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool ContainsKey(TKey key)
    {
      return this.dictionary.ContainsKey(key);
    }

    /// <summary>
    /// O(1)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Add(TKey key, TValue value)
    {
      this[key] = value;
    }

    /// <summary>
    /// O(n)
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Remove(TKey key)
    {
      var resList = this.list.Remove(key);
      var resDic = this.dictionary.Remove(key);

      return resList;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
      return this.dictionary.TryGetValue(key, out value);
    }
  }
}
