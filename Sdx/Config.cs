using System.Collections.Generic;
using Sdx.Data;
using System.Text;
using System.IO;

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


    public Tree Get(string fileName)
    {
      return this.CreateTree(fileName, Encoding.GetEncoding("utf-8"), null);
    }

    public Tree Get(string fileName, string extension)
    {
      return this.CreateTree(fileName, Encoding.GetEncoding("utf-8"), extension);
    }

    public Tree Get(string fileName, Encoding encoding)
    {
      return this.CreateTree(fileName, encoding, null);
    }

    public Tree Get(string fileName, string extension, Encoding encoding)
    {
      return this.CreateTree(fileName, encoding, extension);
    }

    public void ClearCache()
    {
      memoryCache.Clear();
    }

    private Tree CreateTree(string fileName, Encoding encoding, string extension = null)
    {
      //置換しないとBaseを`/path/to/foo`まで含めたGet("bar")と、`/path/to`の時のGet("foo/bar")が
      //同じファイルを指しているのに別のキャッシュになる
      fileName = fileName.Replace('/', Path.DirectorySeparatorChar);
      Tree tree = new T();
      if (extension == null)
      {
        extension = tree.DefaultFileExtension;
      }

      var path = BaseDir + Path.DirectorySeparatorChar + fileName + "." + extension;

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