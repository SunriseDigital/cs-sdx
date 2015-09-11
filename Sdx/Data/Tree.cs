using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sdx.Data
{
  public abstract class Tree
  {
    ///////////////////////////////////////////////////////////////

    public abstract string Value { get; }

    public abstract List<Tree> List { get; }

    protected abstract Tree Get(string path, List<string> paths);

    public abstract void Load(StreamReader input);

    internal abstract string DefaultFileExtension { get; }

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
      return Get(path, paths);
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
