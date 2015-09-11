using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sdx.Data
{
  public abstract class Tree
  {
    private List<Tree> listCache;
    private Dictionary<string, Tree> treeCache = new Dictionary<string, Tree>();

    public abstract void Load(TextReader input);

    internal abstract string DefaultFileExtension { get; }

    /// <summary>
    /// 値を返します。末端のノードでない場合は例外を投げてください。
    /// </summary>
    public abstract string ToValue();

    /// <summary>
    /// Treeのリストを生成して返す。自分自身がListのノードでない場合は例外を投げてください。
    /// </summary>
    /// <returns></returns>
    protected abstract List<Tree> ToList();

    /// <summary>
    /// 自分自身からpaths分階層をだとったノードを含む新しいTreeを生成して返してください。
    /// </summary>
    /// <param name="paths"></param>
    /// <returns></returns>
    protected abstract Tree BuildTree(List<string> paths);

    public string Value
    {
      get
      {
        return this.ToValue();
      }
    }

    public List<Tree> List
    {
      get
      {
        if (this.listCache == null)
        {
          this.listCache = this.ToList();
        }

        
        return this.listCache;
      }
    }

    public Tree Get(string path)
    {
      if(!this.treeCache.ContainsKey(path))
      {
        var paths = this.SplitPath(path);
        this.treeCache[path] = this.BuildTree(paths);
      }
      return this.treeCache[path];
    }


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
