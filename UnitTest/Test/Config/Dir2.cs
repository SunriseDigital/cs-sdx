using System.IO;

namespace Test.Config
{
  class Dir2 : Sdx.Config.TreeYaml
  {
    public Dir2()
    {
      this.BaseDir = Path.GetFullPath(".")
        + Path.DirectorySeparatorChar
        + "config"
        + Path.DirectorySeparatorChar
        + "dir2"
        ;
    }
  }
}
