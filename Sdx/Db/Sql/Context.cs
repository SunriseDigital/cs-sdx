using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Sdx.Db.Sql
{
  public class Context : ICloneable
  {
    public Context(Select select)
    {
      this.Select = select;
    }

    /// <summary>
    /// string|Sdx.Db.Query.Expr|Sdx.Db.Query.Select
    /// </summary>
    public object Target { get; internal set; }

    public string Alias { get; internal set; }

    public Select Select { get; internal set; }

    public string Name
    {
      get 
      {
        if (this.Alias != null)
        {
          return this.Alias;
        }

        return this.Target.ToString(); 
      }
    }

    private Context AddJoin(object target, JoinType joinType, Condition condition, string alias = null)
    {
      Context joinContext = new Context(this.Select);

      joinContext.ParentContext = this;
      joinContext.Target = target;
      joinContext.Alias = alias;
      joinContext.JoinCondition = condition;
      joinContext.JoinType = joinType;

      this.Select.RemoveContext(joinContext.Name);

      this.Select.ContextList.Add(joinContext);
      return joinContext;
    }

    public Context InnerJoin(Table target, Condition condition = null, string alias = null)
    {
      var context = this.AddJoin(target.OwnMeta.Name, JoinType.Inner, condition, alias);

      if(condition == null)
      {
        var contextName = alias == null ? target.OwnMeta.Name : alias;
        context.JoinCondition = this.Table.OwnMeta.CreateJoinCondition(contextName);
      }

      context.Table = target;
      target.Context = context;
      target.AddAllColumnsFromMeta();
      return context;
    }

    public Context InnerJoin(Sdx.Db.Table target, string alias)
    {
      return this.InnerJoin(target, null, alias);
    }

    public Context LeftJoin(Sdx.Db.Table target, Condition condition = null, string alias = null)
    {
      var context = this.AddJoin(target.OwnMeta.Name, JoinType.Left, condition, alias);

      if (condition == null)
      {
        var contextName = alias == null ? target.OwnMeta.Name : alias;
        context.JoinCondition = this.Table.OwnMeta.CreateJoinCondition(contextName);
      }

      context.Table = target;
      target.Context = context;
      target.AddAllColumnsFromMeta();
      return context;
    }

    public Context LeftJoin(Sdx.Db.Table target, string alias)
    {
      return this.LeftJoin(target, null, alias);
    }


    public Context InnerJoin(Select target, Condition condition, string alias = null)
    {
      return this.AddJoin(target, JoinType.Inner, condition, alias);
    }

    public Context LeftJoin(Select target, Condition condition, string alias = null)
    {
      return this.AddJoin(target, JoinType.Left, condition, alias);
    }

    public Context InnerJoin(Expr target, Condition condition, string alias = null)
    {
      return this.AddJoin(target, JoinType.Inner, condition, alias);
    }

    public Context LeftJoin(Expr target, Condition condition, string alias = null)
    {
      return this.AddJoin(target, JoinType.Left, condition, alias);
    }

    public Context InnerJoin(string target, Condition condition, string alias = null)
    {
      return this.AddJoin(target, JoinType.Inner, condition, alias);
    }

    public Context LeftJoin(string target, Condition condition, string alias = null)
    {
      return this.AddJoin(target, JoinType.Left, condition, alias);
    }

    public Context ParentContext { get; private set; }

    internal Condition JoinCondition { get; private set; }

    internal JoinType JoinType { get; set; }

    public Context ClearColumns()
    {
      this.Select.ClearColumns(this.Name);
      return this;
    }

    /// <summary>
    /// エイリアスの付与はできません。
    /// </summary>
    /// <param name="columns">Sdx.Adapter.Query.Expr[]|String[] 配列の中にExprを混ぜられるようにobjectなってます。</param>
    /// <returns></returns>
    public Context AddColumns(params object[] columns)
    {
      foreach (var column in columns)
      {
        this.AddColumn(column);
      }
      return this;
    }

    public Context AddColumn(object columnName, string alias = null)
    {
      var column = new Column(columnName);
      column.Alias = alias;
      column.ContextName = this.Name;
      this.Select.ColumnList.Add(column);
      return this;
    }

    public string AppendName(string column)
    {
      return this.Select.Adapter.QuoteIdentifier(this.Name) + "." + this.Select.Adapter.QuoteIdentifier(column);
    }

    public Condition Where
    {
      get
      {
        Condition where = this.Select.Where;
        where.Context = this;
        return where;
        //ここは下記のようにするとContextの代入ができません。
        //this.Select.Condition.Context = this;
        //return this.Select.Condition;
        //Select.Writeが下記のような実装になっているからです。
        //public Condition Condition
        //{
        //  get
        //  {
        //    this.where.Context = null;
        //    return this.where;
        //  }
        //}
      }
    }

    public Condition Having
    {
      get
      {
        Condition having = this.Select.Having;
        having.Context = this;
        return having;
      }
    }

    public Context AddGroup(object columnName)
    {
      var column = new Column(columnName);
      column.ContextName = this.Name;
      this.Select.GroupList.Add(column);
      return this;
    }

    public Context AddOrder(object columnName, Order order)
    {
      var column = new Column(columnName);
      column.ContextName = this.Name;
      column.Order = order;
      this.Select.OrderList.Add(column);

      return this;
    }

    public Table Table { get; internal set; }

    public object Clone()
    {
      var cloned = (Context)this.MemberwiseClone();

      if (this.Table != null)
      {
        cloned.Table = (Table)this.Table.Clone();
        cloned.Table.Context = cloned;
      }

      if (this.ParentContext != null)
      {
        cloned.ParentContext = (Context)this.ParentContext.Clone();
      }

      if (this.Target is Select)
      {
        cloned.Target = ((Select)this.Target).Clone();
      }

      return cloned;
    }
  }
}
