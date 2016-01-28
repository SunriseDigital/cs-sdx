using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Collection
{
  public class Holder : IEnumerable<KeyValuePair<string, object>>
  {
    public static Holder Create()
    {
      return new Holder();
    }

    private Dictionary<string, object> vars = new Dictionary<string, object>();

    public bool StrictCheck = true;

    public object this[string key]
    {
      set
      {
        this.vars[key] = value;
      }

      get
      {
        return Get(key);
      }
    }

    public bool ContainsKey(string key)
    {
      return this.vars.ContainsKey(key);
    }

    public Holder Set(string key, object value)
    {
      this.vars[key] = value;
      return this;
    }

    public Holder Add(string key, object value)
    {
      //Addは同じキーがあったとき例外になる
      this.vars.Add(key, value);
      return this;
    }

    public object Get(string key)
    {
      if (StrictCheck)
      {
        return this.vars[key];
      }

      if(this.ContainsKey(key))
      {
        return this.vars[key];
      }

      return null;
    }

    public T As<T>(string key)
    {
      if(StrictCheck)
      {
        return (T)this.vars[key];
      }

      if(this.ContainsKey(key))
      {
        object value = this.Get(key);
        if(value is T)
        {
          return (T)value;
        }
      }

      //空コンストラクタがあったらそれで生成して返す。
      var EmptyConstructor = typeof(T).GetConstructor(Type.EmptyTypes);
      if (EmptyConstructor != null)
      {
        return (T)EmptyConstructor.Invoke(null);
      }

      return default(T);
    }

    /// <summary>
    /// 既にそのキーが存在していればそのインスタンスを返し、なければ`factory()`を呼んで、キーにセットします。
    /// IF文と余計な代入を省略できる省略できるショートカットです。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="factory">T型のインスタンスを返す引数なしの関数</param>
    /// <returns></returns>
    public T As<T>(string key, Func<T> factory)
    {
      if (!this.ContainsKey(key))
      {
        this.Add(key, factory());
      }

      return As<T>(key);
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
      return ((IEnumerable<KeyValuePair<string, object>>)vars).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable<KeyValuePair<string, object>>)vars).GetEnumerator();
    }
  }
}
