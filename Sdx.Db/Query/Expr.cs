using System;

namespace Sdx.Db.Query
{
  public class Expr
  {
    private string unquotedValue;
    private Expr(string unquotedValue)
    {
      this.unquotedValue = unquotedValue;
    }

    public static Expr Wrap(string unquotedValue)
    {
      return new Sdx.Db.Query.Expr(unquotedValue);
    }

    public override string ToString()
    {
      return this.unquotedValue;
    }
  }
}
