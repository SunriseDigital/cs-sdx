using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Collection
{
  public class HolderList : IEnumerable<Holder>
  {
    public static HolderList Create()
    {
      return new HolderList();
    }

    private List<Holder> list;

    public HolderList()
    {
      this.list = new List<Holder>();
    }

    public HolderList Add(Holder holder)
    {
      this.list.Add(holder);
      return this;
    }

    public IEnumerator<Holder> GetEnumerator()
    {
      return this.list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.list.GetEnumerator();
    }
  }
}
