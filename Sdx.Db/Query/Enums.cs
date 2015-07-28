using System;

namespace Sdx.Db.Query
{
  public enum Order
  {
    ASC,
    DESC
  }

  public static class EnumStaticExtension
  {
    public static string SqlString(this Order order)
    {
      string[] strings = { "ASC", "DESC" };
      return strings[(int)order];
    }
  }
}
