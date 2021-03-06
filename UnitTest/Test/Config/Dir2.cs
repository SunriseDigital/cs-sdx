﻿using System.IO;

namespace Test.Config
{
  class Dir2<T> : Sdx.Config<T> where T : Sdx.Data.Tree, new()
  {
    public Dir2()
    {
      this.BaseDir = System.AppDomain.CurrentDomain.BaseDirectory
        + Path.DirectorySeparatorChar
        + "config"
        + Path.DirectorySeparatorChar
        + "dir2"
        ;
    }
  }
}
