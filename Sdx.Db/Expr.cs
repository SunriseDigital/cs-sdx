using System;

namespace Sdx.Db
{
  public class Expr
  {
    private string unquotedValue;
    public Expr(string unquotedValue)
    {
      this.unquotedValue = unquotedValue;
    }

    public override string ToString()
    {
      return this.unquotedValue;
    }
  }
}
