using System.Collections.Generic;
using Sdx.Data;
using System.Text;
using System.IO;
using System;

namespace Sdx
{
  public class Config<T> where T : Tree, new()
  {
    private static Dictionary<string, Tree> memoryCache;

    static Config()
    {
      memoryCache = new Dictionary<string, Tree>();
    }

    public string BaseDir { get; set; }


    public virtual Tree Get(string fileName)
    {
      return this.CreateTree(fileName, null, Encoding.GetEncoding("utf-8"));
    }

    public Tree Get(string fileName, string extension)
    {
      return this.CreateTree(fileName, extension, Encoding.GetEncoding("utf-8"));
    }

    public Tree Get(string fileName, Encoding encoding)
    {
      return this.CreateTree(fileName, null, encoding);
    }

    public Tree Get(string fileName, string extension, Encoding encoding)
    {
      return this.CreateTree(fileName, extension, encoding);
    }

    public void ClearCache()
    {
      memoryCache.Clear();
    }

    private Tree CreateTree(string fileName, string extension, Encoding encoding)
    {
      //置換しないとBaseを`/path/to/foo`まで含めたGet("bar")と、`/path/to`の時のGet("foo/bar")が
      //同じファイルを指しているのに別のキャッシュになる
      fileName = fileName.Replace('/', Path.DirectorySeparatorChar);
      Tree tree = new T();

      var path = BaseDir + Path.DirectorySeparatorChar + fileName;
      if (extension != null)
      {
        path += "." + extension;
      }
      
      if(memoryCache.ContainsKey(path))
      {
        return memoryCache[path];
      }

      using (var fs = new FileStream(path, FileMode.Open))
      {
        var input = new StreamReader(fs, encoding);
        tree.Load(input);
      }

      memoryCache[path] = tree;

      return tree;
    }
  }
}