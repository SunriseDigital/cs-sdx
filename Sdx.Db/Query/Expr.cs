using System;

namespace Sdx.Db.Query
{
  public class Expr
  {
    private object unquotedValue;
    private Expr(object unquotedValue)
    {
      this.unquotedValue = unquotedValue;
    }

    public static Expr Wrap(object unquotedValue)
    {
      return new Sdx.Db.Query.Expr(unquotedValue);
    }

    public override string ToString()
    {
      return this.unquotedValue.ToString();
    }
  }
}
