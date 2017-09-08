using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

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

    private object target;
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

    public Context InnerJoin(Table target, Condition condition, string alias, Action<Context> call)
    {
      call(InnerJoin(target, condition, alias));
      return this;
    }

    public Context InnerJoin(Table target, string alias, Action<Context> call)
    {
      call(InnerJoin(target, alias));
      return this;
    }

    public Context InnerJoin(Table target, Condition condition, Action<Context> call)
    {
      call(InnerJoin(target, condition));
      return this;
    }

    public Context InnerJoin(Table target, Action<Context> call)
    {
      call(InnerJoin(target));
      return this;
    }

    public Context InnerJoin(Table target, Condition condition = null, string alias = null)
    {
      if (alias == null)
      {
        alias = target.OwnMeta.DefaultAlias;
      }
      var context = this.AddJoin(target.OwnMeta.Name, JoinType.Inner, condition, alias);
      context.Table = target;

      if(condition == null)
      {
        context.JoinCondition = CreateJoinCondition(context);
      }

      target.Context = context;
      target.AddAllColumnsFromMeta();
      return context;
    }

    public Context InnerJoin(Sdx.Db.Table target, string alias)
    {
      return this.InnerJoin(target, null, alias);
    }

    public Context InnerJoin(Select target, Condition condition, string alias, Action<Context> call)
    {
      call(InnerJoin(target, condition, alias));
      return this;
    }

    public Context InnerJoin(Select target, Condition condition, Action<Context> call)
    {
      call(InnerJoin(target, condition));
      return this;
    }

    public Context InnerJoin(Select target, Condition condition, string alias = null)
    {
      return this.AddJoin(target, JoinType.Inner, condition, alias);
    }

    public Context InnerJoin(Expr target, Condition condition, string alias, Action<Context> call)
    {
      call(InnerJoin(target, condition, alias));
      return this;
    }

    public Context InnerJoin(Expr target, Condition condition, Action<Context> call)
    {
      call(InnerJoin(target, condition));
      return this;
    }

    public Context InnerJoin(Expr target, Condition condition, string alias = null)
    {
      return this.AddJoin(target, JoinType.Inner, condition, alias);
    }

    public Context InnerJoin(string target, Condition condition, string alias, Action<Context> call)
    {
      call(InnerJoin(target, condition, alias));
      return this;
    }

    public Context InnerJoin(string target, Condition condition, Action<Context> call)
    {
      call(InnerJoin(target, condition));
      return this;
    }

    public Context InnerJoin(string target, Condition condition, string alias = null)
    {
      return this.AddJoin(target, JoinType.Inner, condition, alias);
    }

    public Context LeftJoin(Table target, Condition condition, string alias, Action<Context> call)
    {
      call(LeftJoin(target, condition, alias));
      return this;
    }

    public Context LeftJoin(Table target, Condition condition, Action<Context> call)
    {
      call(LeftJoin(target, condition));
      return this;
    }

    public Context LeftJoin(Table target, string alias, Action<Context> call)
    {
      call(LeftJoin(target, alias));
      return this;
    }

    public Context LeftJoin(Table target, Action<Context> call)
    {
      call(LeftJoin(target));
      return this;
    }

    public Context LeftJoin(Table target, Condition condition = null, string alias = null)
    {
      if (alias == null)
      {
        alias = target.OwnMeta.DefaultAlias;
      }
      var context = this.AddJoin(target.OwnMeta.Name, JoinType.Left, condition, alias);
      context.Table = target;

      if (condition == null)
      {
        context.JoinCondition = CreateJoinCondition(context);
      }

      target.Context = context;
      target.AddAllColumnsFromMeta();
      return context;
    }

    private Condition CreateJoinCondition(Context targetContext)
    {
      var cond = new Sql.Condition();

      Table.Relation relation = null;
      if (Table.OwnMeta.Relations.ContainsKey(targetContext.Name))
      {
        relation = Table.OwnMeta.Relations[targetContext.Name];
      }
      else if (Table.OwnMeta.Relations.ContainsKey(targetContext.Target.ToString()))
      {
        relation = Table.OwnMeta.Relations[targetContext.Target.ToString()];
      }
      else if (targetContext.Table != null)
      {
        var candidates = Table.OwnMeta.Relations.Where(rel => rel.Value.TableType.IsAssignableFrom(targetContext.Table.GetType()));
        //if(candidates.Any() && !candidates.Skip(1).Any())
        //とも書けるが、3つ以上同じテーブルのリレーションを張る可能性が極めて低く、また、Countの方が直感的で読みやすい。
        //ベンチもとってみた。
        //https://github.com/SunriseDigital/cs-sdx/pull/134#issuecomment-323269774
        var count = candidates.Count();
        if (count == 1)
        {
          relation = candidates.First().Value;
        }
      }

      if (relation == null)
      {
        throw new InvalidOperationException("Unable to uniquely identify the relation in " + this.Name + " for " + targetContext.Name);
      }

      cond.Add(
        new Sql.Column(relation.ForeignKey, Name),
        new Sql.Column(relation.ReferenceKey, targetContext.Name)
      );

      return cond;
    }

    public Context LeftJoin(Sdx.Db.Table target, string alias)
    {
      return this.LeftJoin(target, null, alias);
    }

    public Context LeftJoin(Select target, Condition condition, string alias, Action<Context> call)
    {
      call(LeftJoin(target, condition, alias));
      return this;
    }

    public Context LeftJoin(Select target, Condition condition, Action<Context> call)
    {
      call(LeftJoin(target, condition));
      return this;
    }

    public Context LeftJoin(Select target, Condition condition, string alias = null)
    {
      return this.AddJoin(target, JoinType.Left, condition, alias);
    }

    public Context LeftJoin(Expr target, Condition condition, string alias, Action<Context> call)
    {
      call(LeftJoin(target, condition, alias));
      return this;
    }

    public Context LeftJoin(Expr target, Condition condition, Action<Context> call)
    {
      call(LeftJoin(target, condition));
      return this;
    }

    public Context LeftJoin(Expr target, Condition condition, string alias = null)
    {
      return this.AddJoin(target, JoinType.Left, condition, alias);
    }

    public Context LeftJoin(string target, Condition condition, string alias, Action<Context> call)
    {
      call(LeftJoin(target, condition, alias));
      return this;
    }

    public Context LeftJoin(string target, Condition condition, Action<Context> call)
    {
      call(LeftJoin(target, condition));
      return this;
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
    /// <param name="columns">Expr|String|Select 配列の中に混ぜられるようにobjectなってます。</param>
    /// <returns></returns>
    public Context AddColumns(params object[] columns)
    {
      foreach (var column in columns)
      {
        this.Select.ColumnList.Add(new Column((dynamic)column, this.Name));
      }
      return this;
    }

    public Context AddColumn(Select select, string alias = null)
    {
      this.Select.ColumnList.Add(new Column(select, this.Name, alias));
      return this;
    }

    public Context AddColumn(Expr expr, string alias = null)
    {
      //exprの時はテーブル名はつけないのでContextNameはnull
      this.Select.ColumnList.Add(new Column(expr, null, alias));
      return this;
    }

    public Context AddColumn(string columnName, string alias = null)
    {
      this.Select.ColumnList.Add(new Column(columnName, this.Name, alias));
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
    /// <param name="expr"></param>
    /// <returns></returns>
    public Context AddGroup(Expr expr)
    {
      var column = new Column(expr, this.Name);
      this.Select.GroupList.Add(column);
      return this;
    }

    public Context AddGroup(string columnName)
    {
      var column = new Column(columnName, this.Name);
      this.Select.GroupList.Add(column);
      return this;
    }

    public Context AddGroup(Select select)
    {
      var column = new Column(select, this.Name);
      this.Select.GroupList.Add(column);
      return this;
    }

    /// <summary>
    /// 全てのカラムをGroupに
    /// </summary>
    /// <returns></returns>
    public Context AddColumnsToGroup(Predicate<Table.Column> predicate = null)
    {
      foreach (var column in Table.OwnMeta.Columns)
      {
        if (predicate == null ? true : predicate(column))
        {
          AddGroup(column.Name);
        }
      }

      return this;
    }

    /// <summary>
    /// 渡ってきたカラム名をGroupに追加
    /// </summary>
    /// <returns></returns>
    public Context AddColumnsToGroup(IEnumerable<string> columns)
    {
      foreach (var column in columns)
      {
        AddGroup(column);
      }

      return this;
    }

    /// <summary>
    /// ORDER句を付与します。カラム名にこの<see cref="Context"/>の名前が付与されます。
    /// </summary>
    /// <param name="expr"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    public Context AddOrder(Expr expr, Order order)
    {
      var column = new Column(expr, this.Name);
      column.Order = order;
      this.Select.OrderList.Add(column);

      return this;
    }

    public Context AddOrder(Select select, Order order)
    {
      var column = new Column(select, this.Name);
      column.Order = order;
      this.Select.OrderList.Add(column);

      return this;
    }

    public Context AddOrder(string columnName, Order order)
    {
      var column = new Column(columnName, this.Name);
      column.Order = order;
      this.Select.OrderList.Add(column);

      return this;
    }

    public Context AddOrderRandom()
    {
      var column = new Column(Expr.Wrap(Select.Adapter.RandomOrderKeyword()));
      column.Order = null;
      Select.OrderList.Add(column);

      return this;
    }

    public Context AddOrderRandom(int seed, string columnName = null)
    {
      var column = new Column(Expr.Wrap(Select.Adapter.RandomOrderKeyword(seed, new Column(columnName, Name))));
      column.Order = null;
      Select.OrderList.Add(column);

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

    public T GetTable<T>() where T:Sdx.Db.Table
    {
      return (T)Table;
    }
  }
}
