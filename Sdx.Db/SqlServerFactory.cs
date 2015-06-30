using System;
using System.Data.Common;

namespace Sdx.Db
{
  public class SqlServerFactory : Factory
  {
    override protected DbProviderFactory GetFactory()
    {
      return DbProviderFactories.GetFactory("System.Data.SqlClient");
    }
  }
}
