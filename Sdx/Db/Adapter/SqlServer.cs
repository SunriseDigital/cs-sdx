﻿using System;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace Sdx.Db.Adapter
{
  public class SqlServer : Base
  {
    override protected DbProviderFactory GetFactory()
    {
      return DbProviderFactories.GetFactory("System.Data.SqlClient");
    }

    protected override string SecureConnectionString
    {
      get
      {
        return Regex.Replace(this.ConnectionString, "(P|p)assword=[^;]+", "${1}assword=" + PasswordForSecureConnectionString);
      }
    }

    internal override void InitSelectEvent(Sql.Select select)
    {
      //AfterFromFunc(ForUpdate)
      select.AfterFromFunc = (sel) => {
        if (sel.ForUpdate)
        {
          return " WITH (UPDLOCK,ROWLOCK)";
        }

        return "";
      };

      //AfterOrderFunc(Limit/Offset)
      select.AfterOrderFunc = (sel) => {
        if (sel.Limit >= 0)
        {
          if (select.OrderList.Count == 0)
          {
            throw new InvalidOperationException("Needs ORDER BY statement to use OFFSET/LIMIT on SQLServer.");
          }
          return " OFFSET " + sel.Offset + " ROWS FETCH NEXT " + sel.Limit + " ROWS ONLY";
        }

        return "";
      };
    }

    internal override object FetchLastInsertId(Connection connection)
    {
      var command = connection.CreateCommand();
      command.CommandText = "SELECT @@IDENTITY";
      return connection.ExecuteScalar(command);
    }

    internal override string RandomOrderKeyword
    {
      get { return "NEWID()"; }
    }
  }
}
