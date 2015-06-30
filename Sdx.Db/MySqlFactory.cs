using System;
using System.Data.Common;

namespace Sdx.Db
{
  public class MySqlFactory : Factory
  {
    override protected DbProviderFactory GetFactory()
    {
      return DbProviderFactories.GetFactory("MySql.Data.MySqlClient");
    }
  }
}
