using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Sdx.Db
{
  public class DbException : System.Data.Common.DbException
  {
    public DbException(string message):base(message)
    {
      
    }

    public DbException(string message, Exception innerException)
      :base(message, innerException)
    {

    }

    public DbException(string message, int errorCode)
      : base(message, errorCode)
    {

    }

    public string ErrorType
    {
      get
      {
        if (InnerException is SqlException)
        {
          var ex = (SqlException)InnerException;
          return ex.Number.ToString();
        }
        else if (InnerException is MySqlException)
        {
          var ex = (MySqlException)InnerException;
          return ex.Number.ToString();
        }

        return "n/a";
      }
    }
  }
}
