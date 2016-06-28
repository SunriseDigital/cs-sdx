using System;

namespace Sdx.Db.Sql
{
  public enum Order
  {
    ASC,
    DESC
  }

  internal enum JoinType
  {
    From,
    Inner,
    Left
  };

  internal enum Logical
  {
    And,
    Or
  }

  public enum JoinOrder
  {
    InnerFront,
    Natural
  }

  public enum Comparison
  {
    Equal,
    NotEqual,
    AltNotEqual,
    GreaterThan,
    LessThan,
    GreaterEqual,
    LessEqual,
    Like,
    NotLike,
    In,
    NotIn,
  }

  internal static class Enumns
  {
    internal static string SqlString(this Logical? logical)
    {
      if(logical == null)
      {
        return "";
      }

      string[] strings = { " AND ", " OR " };
      return strings[(int)logical];
    }

    internal static string SqlString(this Comparison? comp)
    {
      if(comp == null)
      {
        return "";
      }

      string[] strings = {
          " = ",
          " <> ",
          " != ",
          " > ",
          " < ",
          " >= ",
          " <= ",
          " LIKE ",
          " NOT LIKE ",
          " IN ",
          " NOT IN ",
        };
      return strings[(int)comp];
    }

    internal static string SqlString(this JoinType gender)
    {
      string[] strings = { "FROM", "INNER JOIN", "LEFT JOIN" };
      return strings[(int)gender];
    }

    internal static string SqlString(this Order order)
    {
      string[] strings = { "ASC", "DESC" };
      return strings[(int)order];
    }
  }
}
