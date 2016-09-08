using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Sdx.Db.Sql
{
  public class Column
  {
    private object target;

    public string Alias { get; internal set; }

    internal string ContextName { get; set; }

    internal Order? Order { get; set; }

    /// <summary>
    /// Update/Insertにおいて、更新対象の値を保持するのに使う
    /// </summary>
    internal object Value { get; set; }

    public Column(Expr expr, string contextName = null, string alias = null)
    {
      target = expr;
      ContextName = contextName;
      Alias = alias;
    }

    public Column(Select select, string contextName = null, string alias = null)
    {
      target = select;
      ContextName = contextName;
      Alias = alias;
    }

    public Column(string columnName, string contextName = null, string alias = null)
    {
      if (columnName == "*")
      {
        target = Expr.Wrap(columnName);
      }
      else
      {
        target = columnName;
      }

      ContextName = contextName;
      Alias = alias;
    }

    public object Target
    {
      get { return this.target; }
    }

    public string Name
    {
      get
      {
        if(this.Alias != null)
        {
          return this.Alias;
        }

        return this.target.ToString();
      }
    }

    private string QuotedName(Adapter.Base db, DbParameterCollection parameters, Counter condCount)
    {
      if(this.target is Expr)
      {
        return db.QuoteIdentifier(this.target as Expr);
      }
      else if(this.target is Select)
      {
        var select = this.target as Select;
        return "(" + select.BuildSelectString(parameters, condCount) + ")";
      }
      else
      {
        return db.QuoteIdentifier(this.target as String);
      }
    }

    internal string Build(Adapter.Base db, DbParameterCollection parameters, Counter condCount)
    {
      var sql = "";
      if(this.ContextName != null)
      {
        sql = db.QuoteIdentifier(this.ContextName) + "." + this.QuotedName(db, parameters, condCount);
      }
      else
      {
        sql = this.QuotedName(db, parameters, condCount);
      }

      if(this.Alias != null)
      {
        sql += " AS " + db.QuoteIdentifier(this.Alias);
      }

      return sql;
    }

    /// <summary>
    /// 同じカラムかどうかを返す。何に使われているかや、値、エイリアスは考慮しません。コンテキスト中で同じカラムを指しているかのみチェックします。
    /// ContextNameとtargetを比べます。サブクエリーの場合はSelectが同じオブジェクトかどうかを返す。
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    internal bool SameAs(Column column)
    {
      if(target is Select)
      {
        return target == column.target;
      }

      if(ContextName != column.ContextName)
      {
        return false;
      }

      return target.ToString() == column.target.ToString();
    }
  }
}
