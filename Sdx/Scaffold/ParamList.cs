using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Scaffold
{
  public class ParamList : IEnumerable<Param>
  {
    public static ParamList Create()
    {
      return new ParamList();
    }

    private List<Param> list = new List<Param>();

    public Param this[int index]
    {
      get
      {
        return list[index];
      }
    }

    public ParamList Add(Param param)
    {
      this.list.Add(param);
      return this;
    }

    public IEnumerator<Param> GetEnumerator()
    {
      return this.list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.list.GetEnumerator();
    }
  }
}
