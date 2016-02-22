using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Scaffold
{
  public class ConfigList : IEnumerable<ConfigItem>
  {
    public static ConfigList Create()
    {
      return new ConfigList();
    }

    private List<ConfigItem> list = new List<ConfigItem>();

    public ConfigItem this[int index]
    {
      get
      {
        return list[index];
      }
    }

    public ConfigList Add(ConfigItem param)
    {
      this.list.Add(param);
      return this;
    }

    public IEnumerator<ConfigItem> GetEnumerator()
    {
      return this.list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.list.GetEnumerator();
    }
  }
}
