using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Sdx.Db.Sql
{
  /// <summary>
  /// <see cref="Sql.Select"/>の中でFROM句やJOINしたテーブルを表現するクラス。
  /// <see cref="Db.Table"/>と明確に区別するためContextという名前になってます。
  /// またテーブルだけではなくJOINしたサブクエリーも<see cref="Context"/>です。
  /// </summary>
  public class Context : ICloneable
  {
    public Context(Select select)
    {
      this.Select = select;
    }

    /// <summary>
    /// 対象のテーブルまたはサブクエリー。型は
    /// <see cref="string"/>|<see cref="Expr"/>|<see cref="Sql.Select"/>です。
    /// </summary>
    public object Target { get; internal set; }

    public string Alias { get; internal set; }

    internal Select Select { get; set; }

    /// <summary>
    /// <see cref="Alias"/>があったら<see cref="Alias"/>。なかったら<see cref="Target"/>の文字列表現を返す。
    /// </summary>
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

    /// <summary>
    /// Joinの起点メソッド
    /// </summary>
    /// <param name="target"></param>
    /// <param name="joinType"></param>
    /// <param name="condition"></param>
    /// <param name="alias"></param>
    /// <returns></returns>
    private Context AddJoin(object target, JoinType joinType, Condition condition, string alias = null)
    {
      Context joinContext = this.Select.CreateContext(target, alias, joinType);

      joinContext.ParentContext = this;
      joinContext.JoinCondition = condition;

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

    public Context InnerJoin(Select target, Condition condition, string alias = null)
    {
      return this.AddJoin(target, JoinType.Inner, condition, alias);
    }

    public Context InnerJoin(Expr target, Condition condition, string alias = null)
    {
      return this.AddJoin(target, JoinType.Inner, condition, alias);
    }

    public Context InnerJoin(string target, Condition condition, string alias = null)
    {
      return this.AddJoin(target, JoinType.Inner, condition, alias);
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

    public Context LeftJoin(Select target, Condition condition, string alias = null)
    {
      return this.AddJoin(target, JoinType.Left, condition, alias);
    }

    public Context LeftJoin(Expr target, Condition condition, string alias = null)
    {
      return this.AddJoin(target, JoinType.Left, condition, alias);
    }

    public Context LeftJoin(string target, Condition condition, string alias = null)
    {
      return this.AddJoin(target, JoinType.Left, condition, alias);
    }

    /// <summary>
    /// JOINしたテーブルの場合、JOIN先テーブルの<see cref="Context"/>を返します。
    /// FROM句のテーブルだった場合はNULLが返ります。
    /// </summary>
    internal Context ParentContext { get; set; }

    internal Condition JoinCondition { get; private set; }

    internal JoinType JoinType { get; set; }

    /// <summary>
    /// <see cref="Sql.Select"/>からこのテーブルのカラムをすべて除きます。
    /// </summary>
    /// <returns></returns>
    public Context ClearColumns()
    {
      this.Select.ClearColumns(this.Name);
      return this;
    }

    /// <summary>
    /// <see cref="Sql.Select"/>にこのテーブルのテーブル名付きカラムを複数追加します。
    /// エイリアスの付与はできません。
    /// </summary>
    /// <param name="columns">Sdx.Adapter.Query.Expr[]|String[] 配列の中にExprを混ぜられるようにobjectなってます。</param>
    /// <returns></returns>
    public Context AddColumns(params object[] columns)
    {
      foreach (var column in columns)
      {
        this.AddColumnObject(column);
      }
      return this;
    }

    public Context AddColumn(Select subquery, string alias = null)
    {
      this.AddColumnObject(subquery, alias);
      return this;
    }

    public Context AddColumn(Expr expr, string alias = null)
    {
      this.AddColumnObject(expr, alias);
      return this;
    }

    public Context AddColumn(string columnName, string alias = null)
    {
      this.AddColumnObject(columnName, alias);
      return this;
    }

    public Context SetColumns(params object[] columns)
    {
      this.ClearColumns();
      this.AddColumns(columns);
      return this;
    }

    public Context SetColumn(Select subquery, string alias = null)
    {
      this.ClearColumns();
      this.AddColumn(subquery, alias);
      return this;
    }

    public Context SetColumn(Expr expr, string alias = null)
    {
      this.ClearColumns();
      this.AddColumn(expr, alias);
      return this;
    }

    public Context SetColumn(string columnName, string alias = null)
    {
      this.ClearColumns();
      this.AddColumn(columnName, alias);
      return this;
    }

    /// <summary>
    /// <see cref="Sql.Select"/>にこのテーブルのカラムを一つ追加します。
    /// </summary>
    /// <param name="columnName"></param>
    /// <param name="alias"></param>
    /// <returns></returns>
    internal Context AddColumnObject(object columnName, string alias = null)
    {
      var column = new Column(columnName);
      column.Alias = alias;
      column.ContextName = this.Name;
      this.Select.ColumnList.Add(column);
      return this;
    }

    /// <summary>
    /// カラム名にこの<see cref="Context"/>の名前をクオートして付与します。カラム名もクオートされます。
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public string AppendName(string column)
    {
      return this.Select.Adapter.QuoteIdentifier(this.Name) + "." + this.Select.Adapter.QuoteIdentifier(column);
    }

    /// <summary>
    /// <see cref="Sql.Select"/>にWHERE句を付与します。
    /// このプロパティー経由で付与されるWHERE句はカラム名にこの<see cref="Context"/>の名前が付与されます。
    /// </summary>
    public Condition Where
    {
      get
      {
        Condition where = this.Select.Where;
        where.ContextName = this.Name;
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

    public Context WhereCall(Action<Condition> callback)
    {
      callback.Invoke(Where);
      return this;
    }

    /// <summary>
    /// <see cref="Sql.Select"/>にHAVING句を付与します。
    /// このプロパティー経由で付与されるHAVING句はカラム名にこの<see cref="Context"/>の名前が付与されます。
    /// </summary>
    public Condition Having
    {
      get
      {
        Condition having = this.Select.Having;
        having.ContextName = this.Name;
        return having;
      }
    }

    /// <summary>
    /// GROUP句を付与します。カラム名にこの<see cref="Context"/>の名前が付与されます。
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns></returns>
    public Context AddGroup(object columnName)
    {
      var column = new Column(columnName);
      column.ContextName = this.Name;
      this.Select.GroupList.Add(column);
      return this;
    }

    /// <summary>
    /// ORDER句を付与します。カラム名にこの<see cref="Context"/>の名前が付与されます。
    /// </summary>
    /// <param name="columnName"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    public Context AddOrder(object columnName, Order order)
    {
      var column = new Column(columnName);
      column.ContextName = this.Name;
      column.Order = order;
      this.Select.OrderList.Add(column);

      return this;
    }

    /// <summary>
    /// <see cref="Db.Table"/>を使ってFROM句/JOIN句に追加された<see cref="Context"/>からはこのプロパティーから<see cref="Db.Table"/>が取得できます。
    /// </summary>
    public Table Table { get; internal set; }

    public object Clone()
    {
      var cloned = (Context)this.MemberwiseClone();

      if (this.Table != null)
      {
        cloned.Table = (Table)this.Table.Clone();
        cloned.Table.Context = cloned;
      }

      if (this.Target is Select)
      {
        cloned.Target = ((Select)this.Target).Clone();
      }

      cloned.Select = null;
      cloned.ParentContext = null;
      /// <see cref="Select"/>と<see cref="ParentContext"/>は<see cref="Sql.Select.Clone"/>ですげ替えを行っています。

      return cloned;
    }
  }
}
