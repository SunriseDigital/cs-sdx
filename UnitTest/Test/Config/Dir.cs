using System.IO;

namespace Test.Config
{
  class Dir : Sdx.Config.TreeYaml
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
