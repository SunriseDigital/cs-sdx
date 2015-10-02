using System.IO;

namespace Test.Config
{
  class Dir2<T> : Sdx.Config<T> where T : Sdx.Data.Tree, new()
  {
    public Dir2()
    {
      this.BaseDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
        + Path.DirectorySeparatorChar
        + "config"
        + Path.DirectorySeparatorChar
        + "dir2"
        ;
    }
  }
}
