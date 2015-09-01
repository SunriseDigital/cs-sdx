﻿using System;

namespace Sdx.Db.Query
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
    IsNull,
    IsNotNull
  }

  internal static class Enumns
  {
    internal static string SqlString(this Logical logical)
    {
      string[] strings = { " AND ", " OR " };
      return strings[(int)logical];
    }

    internal static string SqlString(this Comparison comp)
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
          " NOT IN ",
          " IS NULL",
          " IS NOT NULL"
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
