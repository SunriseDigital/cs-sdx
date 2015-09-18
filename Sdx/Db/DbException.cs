using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Db
{
  public class DbException : System.Data.Common.DbException
  {
    public DbException(string message):base(message)
    {
      
    }
  }
}
