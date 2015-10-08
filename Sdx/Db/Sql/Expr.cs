using System;

namespace Sdx.Db.Sql
{
  public class Expr
  {
    private object unquotedValue;
    //TODO これってobject?　StringでOKじゃないか？
    private Expr(object unquotedValue)
    {
      this.unquotedValue = unquotedValue;
    }

    public static Expr Wrap(object unquotedValue)
    {
      return new Sdx.Db.Sql.Expr(unquotedValue);
    }

    public override string ToString()
    {
      return this.unquotedValue.ToString();
    }
  }
}
