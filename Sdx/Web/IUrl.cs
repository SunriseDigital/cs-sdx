using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Web
{
  public interface IUrl
  {
    IUrl Next(string key, int defaultValue = 1);
    IUrl Prev(string key);
    IUrl Add(string key, string value);
    IUrl Remove(string key);
    IUrl Set(string key, string value);
    IUrl RemoveAll(params string[] excepts);
    IUrl ReplaceKey(string from, string to);
    string Build();
    Url ToUrl();
  }
}
