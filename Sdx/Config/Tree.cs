using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Config
{
  public abstract class Tree
  {
    ///////////////////////////////////////////////////////////////
    public string BaseDir { get; set; }

    public abstract string Value { get; }

    public abstract List<Tree> List { get; }

    protected abstract Tree Get(List<string> paths);

    private List<string> SplitPath(string path)
    {
      var paths = new List<string>();
      var index = 0;
      //`..`と二つ重ねると`.`を表現できるようにするため
      string prevForEscaped = null;
      foreach (var chunk in path.Split('.'))
      {
        if (chunk == "")
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
        else if (prevForEscaped != null)
        {
          paths.Add(prevForEscaped + "." + chunk);
          prevForEscaped = null;

        }
        else
        {
          paths.Add(chunk);
        }
        ++index;
      }

      return paths;
    }

    public Tree Get(string path)
    {
      var paths = this.SplitPath(path);
      return Get(paths);
    }

    public IEnumerator<Tree> GetEnumerator()
    {
      return ((IEnumerable<Tree>)List).GetEnumerator();
    }

    public int Count
    {
      get
      {
        return this.List.Count;
      }
    }

    public Tree this[int i]
    {
      get
      {
        return this.List[i];
      }
    }
  }
}
