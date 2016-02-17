using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Scaffold
{
  public class ParamsList : IEnumerable<Params>
  {
    public static ParamsList Create()
    {
      return new ParamsList();
    }

    private List<Params> list = new List<Params>();

    public Params this[int index]
    {
      get
      {
        return list[index];
      }
    }

    public ParamsList Add(Params param)
    {
      this.list.Add(param);
      return this;
    }

    public IEnumerator<Params> GetEnumerator()
    {
      return this.list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.list.GetEnumerator();
    }
  }
}
