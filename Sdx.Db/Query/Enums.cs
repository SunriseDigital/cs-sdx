using System;

namespace Sdx.Db.Query
{
  public enum Order
  {
    ASC,
    DESC
  }

  public enum JoinType
  {
    From,
    Inner,
    Left
  };

  public enum JoinOrder
  {
    InnerFront,
    Natural
  }

  public enum Logical
  {
    And,
    Or
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
    NotIn
  }

  public static class Enumns
  {
    public static string SqlString(this Logical logical)
    {
      string[] strings = { " AND ", " OR " };
      return strings[(int)logical];
    }

    public static string SqlString(this Comparison comp)
    {
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
          " NOT IN "
        };
      return strings[(int)comp];
    }

    public static string SqlString(this JoinType gender)
    {
      string[] strings = { "FROM", "INNER JOIN", "LEFT JOIN" };
      return strings[(int)gender];
    }

    public static string SqlString(this Order order)
    {
      string[] strings = { "ASC", "DESC" };
      return strings[(int)order];
    }
  }
}
