﻿using System;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace Sdx.Db.Adapter
{
  public class MySql : Base
  {
    override protected DbProviderFactory GetFactory()
    {
      return DbProviderFactories.GetFactory("MySql.Data.MySqlClient");
    }

    internal override void InitSelectEvent(Sql.Select select)
    {
      //AfterOrderFunc(Limit/Offset+ForUpdate)
      select.AfterOrderFunc = (sel) => {
        var result = "";

        if (sel.Limit >= 0)
        {
          result += " LIMIT " + sel.Limit;
          if (sel.Offset > 0)
          {
            result += " OFFSET " + sel.Offset;
          }
        }

        if (sel.ForUpdate)
        {
          result += " FOR UPDATE";
        }

        return result;
      };
    }

    protected override string SecureConnectionString
    {
      get
      {
        return Regex.Replace(this.ConnectionString, "(P|p)wd=[^;]+", "${1}wd=" + PasswordForSecureConnectionString);
      }
    }

    internal override object FetchLastInsertId(Connection connection)
    {
      var command = connection.CreateCommand();
      command.CommandText = "SELECT LAST_INSERT_ID()";
      return connection.ExecuteScalar(command);
    }
  }
}