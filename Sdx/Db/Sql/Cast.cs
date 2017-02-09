using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Db.Sql
{
  public class Cast
  {
    public string Column { get; private set; }

    public string Type { get; private set; }

    public Cast(string column, string type)
    {
      Column = column;
      Type = type;
    }
  }
}
