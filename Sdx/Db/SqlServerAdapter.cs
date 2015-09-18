using System;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace Sdx.Db
{
  public class SqlServerAdapter : Adapter
  {
    override protected DbProviderFactory GetFactory()
    {
      return DbProviderFactories.GetFactory("System.Data.SqlClient");
    }

    protected override string SecureConnectionString
    {
      get
      {
        return Regex.Replace(this.ConnectionString, "(P|p)assword=[^;]+", "${1}assword=" + PWD_FOR_SECURE_CONNECTION_STRING);
      }
    }

    internal override void InitSelectEvent(Query.Select select)
    {
      //AfterFromFunc(ForUpdate)
      select.AfterFromFunc = (sel) => {
        if (sel.ForUpdate)
        {
          return " WITH (updlock)";
        }

        return "";
      };

      //AfterOrderFunc(Limit/Offset)
      select.AfterOrderFunc = (sel) => {
        if (sel.Limit >= 0)
        {
          return " OFFSET " + sel.Offset + " ROWS FETCH NEXT " + sel.Limit + " ROWS ONLY";
        }

        return "";
      };
    }
  }
}
