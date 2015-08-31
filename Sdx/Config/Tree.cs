using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Config
{
  public abstract class Tree
  {
    public string BaseDir { get; set; }

    /// <summary>
    /// ネストした辞書を取得するときGetTreeDicを使うが、このTreeは辞書の場合とStringの場合とListの場合がある。
    /// 辞書に関してはGet系メソッドで取得できるがString/Listの場合はキーがないのでGet系メソッドでは取得できないため
    /// StringValue/StrListValue/TreeListValueで値を取得する
    /// </summary>
    /// <returns></returns>
    public abstract string StringValue { get; }

    public abstract List<string> StrListValue { get; }

    protected abstract string GetString(List<string> paths);

    protected abstract Dictionary<string, string> GetStrDic(List<string> paths);

    protected abstract List<Tree> GetTreeList(List<string> paths);

    protected abstract List<string> GetStrList(List<string> paths);

    protected abstract Dictionary<string, Tree> GetTreeDic(List<string> paths);

    public String GetString(string path)
    {
      var paths = this.SplitPath(path);
      return GetString(paths);
    }

    public Dictionary<string, string> GetStrDic(string path)
    {
      var paths = this.SplitPath(path);
      return GetStrDic(paths);
    }

    public List<Tree> GetTreeList(string path)
    {
      var paths = this.SplitPath(path);
      return GetTreeList(paths);
    }

    public List<string> GetStrList(string path)
    {
      var paths = this.SplitPath(path);
      return GetStrList(paths);
    }

    public Dictionary<string, Tree> GetTreeDic(string path)
    {
      var paths = this.SplitPath(path);
      return GetTreeDic(paths);
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
  }
}
