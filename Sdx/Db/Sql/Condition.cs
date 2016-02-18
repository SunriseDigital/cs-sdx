using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Sdx.Db.Sql
{
  public class Condition : ICloneable
  {
    private enum Type
    {
      Comparison,
      NullCompare,
      Between,
      Free,
    }
    private class Holder
    {
      public Comparison? Comparison { get; set; }
      public Logical? Logical { get; set; }
      public Column Column { get; set; }
      public object Value { get; set; }
      public Type Type { get; set; }
    }

    private List<Holder> wheres = new List<Holder>();
    private List<Condition> childConditions = new List<Condition>();

    internal int Count
    {
      get { return wheres.Count; }
    }

    internal bool NeedsBracket { get; set; }

    internal string ContextName { private get; set; }

    public Condition AddOr(Condition condition)
    {
      this.AddWithCondition(condition, Logical.Or, Type.Free);
      return this;
    }

    public Condition AddOr(string column, Object value, Comparison comparison = Comparison.Equal)
    {
      this.AddWithColumn(column, value, Logical.Or, comparison, Type.Comparison);
      return this;
    }

    public Condition AddOr(Expr column, Object value, Comparison comparison = Comparison.Equal)
    {
      this.AddWithColumn(column, value, Logical.Or, comparison, Type.Comparison);
      return this;
    }

    public Condition Add(Condition condition)
    {
      this.AddWithCondition(condition, Logical.And, Type.Free);
      return this;
    }

    public Condition Add(string column, Object value, Comparison comparison = Comparison.Equal)
    {
      this.AddWithColumn(column, value, Logical.And, comparison, Type.Comparison);
      return this;
    }

    public Condition Add(Expr column, Object value, Comparison comparison = Comparison.Equal)
    {
      this.AddWithColumn(column, value, Logical.And, comparison, Type.Comparison);
      return this;
    }

    public Condition AddIsNull(string column)
    {
      this.AddWithColumn(column, null, Logical.And, Comparison.Equal, Type.NullCompare);
      return this;
    }

    public Condition AddIsNotNullOr(string column)
    {
      this.AddWithColumn(column, null, Logical.Or, Comparison.NotEqual, Type.NullCompare);
      return this;
    }

    public Condition AddIsNullOr(string column)
    {
      this.AddWithColumn(column, null, Logical.Or, Comparison.Equal, Type.NullCompare);
      return this;
    }

    public Condition AddIsNotNull(string column)
    {
      this.AddWithColumn(column, null, Logical.And, Comparison.NotEqual, Type.NullCompare);
      return this;
    }

    /// <summary>
    /// 自分の持っているConditionにContextNameをセット。
    /// </summary>
    /// <param name="contextName"></param>
    /// <param name="context"></param>
    private void FixContext(string contextName)
    {
      this.ContextName = contextName;

      this.wheres.ForEach(holder => {
        if (holder.Column == null) return;
        var cName = contextName == null ? null : contextName;
        holder.Column.ContextName = cName;
      });

      this.childConditions.ForEach(condition => {
        condition.FixContext(contextName);
      });
    }

    private void AddWithCondition(Condition condition, Logical logical, Type type)
    {
      condition.NeedsBracket = true;
      this.childConditions.Add(condition);
      if (this.ContextName != null)
      {
        condition.FixContext(this.ContextName);
      }

      this.Add(new Holder
      {
        Value = condition,
        Logical = logical,
        Type = type
      });
    }

    private void AddWithColumn(Object columnName, Object value, Logical? logical, Comparison? comparison, Type type)
    {
      Column column;
      if (!(columnName is Column))
      {
        column = new Column(columnName);

        if (this.ContextName != null)
        {
          column.ContextName = this.ContextName;
        }
      }
      else
      {
        column = (Column)columnName;
      }

      this.Add(new Holder
      {
        Column = column,
        Logical = logical,
        Comparison = comparison,
        Value = value,
        Type = type,
      });
    }

    private void Add(Holder holder)
    {
      if (holder.Logical == Logical.Or && this.wheres.Count == 0)
      {
        //一番最初のWhereがOrで足されたということは何かプログラマーの意図と違うことが起こってるはず。
        throw new InvalidOperationException("Illegal logical operation for the first where condition `" + holder.Logical.SqlString() + "`");
      }

      this.wheres.Add(holder);
    }

    internal void Build(StringBuilder builder, Adapter.Base adapter, DbParameterCollection parameters, Counter condCount)
    {
      if (this.NeedsBracket)
      {
        builder.Append("(");
      }

      var first = true;
      wheres.ForEach(holder =>
      {
        if (!first)
        {
          builder.Append(holder.Logical.SqlString());
        }
        first = false;

        if (holder.Value is Condition)
        {
          Condition where = holder.Value as Condition;
          where.Build(builder, adapter, parameters, condCount);
        }
        else
        {
          builder.Append(this.BuildConditionString(adapter, parameters, holder, condCount));
        }
      });

      if (this.NeedsBracket)
      {
        builder.Append(")");
      }
    }

    private string BuildPlaceholderAndParameters(Adapter.Base adapter, DbParameterCollection parameters, Holder cond, Counter condCount)
    {
      string rightHand;

      if (cond.Value is Column)
      {
        rightHand = ((Column)cond.Value).Build(adapter, parameters, condCount);
      }
      else if (cond.Value is Expr)
      {
        rightHand = cond.Value.ToString();
        condCount.Incr();
      }
      else if (cond.Value is Select)
      {
        Select sub = (Select)cond.Value;
        if (sub.Adapter == null)
        {
          sub.Adapter = adapter;
        }
        rightHand = "(" + sub.BuildSelectString(parameters, condCount) + ")";
      }
      //IEnumerable<>かどうかチェック。
      else if (!(cond.Value is string) && cond.Value.GetType().GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
      {
        cond.Comparison = Comparison.In;
        string inCond = "";
        var list = cond.Value as IEnumerable<object>;
        foreach (var value in list)
        {
          if (inCond != "")
          {
            inCond += ", ";
          }
          string holder = "@" + condCount.Value;
          parameters.Add(adapter.CreateParameter(holder, value));
          inCond += holder;
          condCount.Incr();
        }

        rightHand = "(" + inCond + ")";
      }
      else
      {
        rightHand = "@" + condCount.Value;
        parameters.Add(adapter.CreateParameter(rightHand, cond.Value));
        condCount.Incr();
      }

      return rightHand;
    }

    private string BuildConditionString(Adapter.Base adapter, DbParameterCollection parameters, Holder cond, Counter condCount)
    {
      if (cond.Type == Type.Comparison)
      {
        var value = this.BuildPlaceholderAndParameters(adapter, parameters, cond, condCount);
        return String.Format(
          "{0}{1}{2}",
          cond.Column.Build(adapter, parameters, condCount),
          cond.Comparison.SqlString(),
          value
        );
      }
      else if(cond.Type == Type.NullCompare)
      {
        if (cond.Comparison == Comparison.Equal)
        {
          return cond.Column.Build(adapter, parameters, condCount) + " IS NULL";
        }
        else if(cond.Comparison == Comparison.NotEqual)
        {
          return cond.Column.Build(adapter, parameters, condCount) + " IS NOT NULL";
        }

        throw new InvalidOperationException("Illeagal Comparison is specified.");
      }
      else if(cond.Type == Type.Between)
      {
        var minMax = (string[])cond.Value;

        cond.Value = minMax[0];
        var min = this.BuildPlaceholderAndParameters(adapter, parameters, cond, condCount);

        cond.Value = minMax[1];
        var max = this.BuildPlaceholderAndParameters(adapter, parameters, cond, condCount);

        //元に戻さないと2回で上のキャストがこけてしまう。
        cond.Value = minMax;

        string condFormat;
        if(cond.Comparison == Comparison.Equal)
        {
          condFormat = "{0} BETWEEN {1} AND {2}";
        }
        else if (cond.Comparison == Comparison.NotEqual)
        {
          condFormat = "{0} NOT BETWEEN {1} AND {2}";
        }
        else
        {
          throw new InvalidOperationException("Illegal Comparison is specified");
        }

        return String.Format(
          condFormat,
          cond.Column.Build(adapter, parameters, condCount),
          min,
          max
        );
      }
      else if(cond.Type == Type.Free)
      {
        return cond.Column.Name;
      }

      throw new InvalidOperationException("Illeagal Type is specified.");
    }

    public Condition Add(Column left, object right, Comparison comparison = Comparison.Equal)
    {
      this.AddWithColumn(left, right, Logical.And, comparison, Type.Comparison);
      return this;
    }

    public Condition AddOr(Column left, object right, Comparison comparison = Comparison.Equal)
    {
      this.AddWithColumn(left, right, Logical.Or, comparison, Type.Comparison);
      return this;
    }

    public object Clone()
    {
      var cloned = (Condition)this.MemberwiseClone();

      cloned.wheres = new List<Holder>(this.wheres);

      cloned.childConditions = new List<Condition>();
      this.childConditions.ForEach(condition =>
      {
        cloned.childConditions.Add((Condition)condition.Clone());
      });

      //ContextはSelectやContextのプロパティ経由でアクセスされたときにセットされる一時的な変数なのでクリアする。
      cloned.ContextName = null;

      return cloned;
    }

    public Condition AddBetween(string column, string min, string max)
    {
      this.AddWithColumn(column, new String[] {min, max}, Logical.And, Comparison.Equal, Type.Between);
      return this;
    }

    public Condition AddBetween(Expr column, string min, string max)
    {
      this.AddWithColumn(column, new String[] { min, max }, Logical.And, Comparison.Equal, Type.Between);
      return this;
    }

    public Condition AddBetweenOr(string column, string min, string max)
    {
      this.AddWithColumn(column, new String[] { min, max }, Logical.Or, Comparison.Equal, Type.Between);
      return this;
    }

    public Condition AddBetweenOr(Expr column, string min, string max)
    {
      this.AddWithColumn(column, new String[] { min, max }, Logical.Or, Comparison.Equal, Type.Between);
      return this;
    }

    public Condition AddNotBetween(string column, string min, string max)
    {
      this.AddWithColumn(column, new String[] { min, max }, Logical.And, Comparison.NotEqual, Type.Between);
      return this;
    }

    public Condition AddNotBetween(Expr column, string min, string max)
    {
      this.AddWithColumn(column, new String[] { min, max }, Logical.And, Comparison.NotEqual, Type.Between);
      return this;
    }

    public Condition AddNotBetweenOr(string column, string min, string max)
    {
      this.AddWithColumn(column, new String[] { min, max }, Logical.Or, Comparison.NotEqual, Type.Between);
      return this;
    }

    public Condition AddNotBetweenOr(Expr column, string min, string max)
    {
      this.AddWithColumn(column, new String[] { min, max }, Logical.Or, Comparison.NotEqual, Type.Between);
      return this;
    }

    /// <summary>
    /// 引数一個のAddは自由文をWhere句に足します。クオートやサニタイズは一切されないので注意してください。
    /// また、AND・ORは付与されません。
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public Condition Add(string expression)
    {
      this.AddWithColumn(expression, null, null, null, Type.Free);
      return this;
    }
  }
}