using System.IO;

namespace Test.Config
{
  class Dir<T> : Sdx.Config<T> where T : Sdx.Data.Tree, new()
  {
    public Dir()
    {
      this.BaseDir = Path.GetFullPath(".")
        + Path.DirectorySeparatorChar
        + "config"
        + Path.DirectorySeparatorChar
        + "dir"
        ;
    }
  }
}
