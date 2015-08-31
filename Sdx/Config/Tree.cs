using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Config
{
  public abstract class Tree
  {
    public string BaseDir { get; set; }

    protected abstract object GetData<T>(List<string> paths);

    public T Get<T>(string path)
    {
      var paths = new List<string>();
      var index = 0;
      //`..`と二つ重ねると`.`を表現できるようにするため
      string prevForEscaped = null;
      foreach(var chunk in path.Split('.'))
      {
        if(chunk == "")
        {
          var prevKey = index - 1;
          if (paths.Count > index)
          {
            prevForEscaped = paths[prevKey];
            paths.RemoveAt(prevKey);
          }
          else
          {
            prevForEscaped = "";
          }

        }
        else if(prevForEscaped != null)
        {
          paths.Add(prevForEscaped + "."+ chunk);
          prevForEscaped = null;

        }
        else
        {
          paths.Add(chunk);
        }
        ++index;
      }

      //if (!this.data.ContainsKey(paths[0]))
      //{
      //  this.data[paths[0]] = this.LoadData(paths[0]);
      //}

      //var target = this.data[paths[0]];
      //foreach(var key in paths)
      //{
      //  target = target.Get(key);
      //}



      return (T)this.GetData<T>(paths);
    }
  }
}
