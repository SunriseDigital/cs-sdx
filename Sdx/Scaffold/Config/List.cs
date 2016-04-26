using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Scaffold.Config
{
  public class List : IEnumerable<Item>
  {
    public static List Create()
    {
      return new List();
    }

    private List<Item> list = new List<Item>();

    public Item this[int index]
    {
      get
      {
        return list[index];
      }
    }

    public List Add(Item param)
    {
      this.list.Add(param);
      return this;
    }

    public bool IsEmpty
    {
      get
      {
        return this.list.Count == 0;
      }
    }

    public IEnumerator<Item> GetEnumerator()
    {
      return this.list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.list.GetEnumerator();
    }
  }
}
