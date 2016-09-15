using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Sdx.Db.Sql
{
  public class Select : ICloneable
  {
    public const string CommentParameterKey = "##Sdx.Db.Query.Select.Comment##";

    private Sdx.Collection.OrderedDictionary<string, Context> contextList = new Sdx.Collection.OrderedDictionary<string, Context>();
    private List<Column> columns = new List<Column>();
    private List<Column> groups = new List<Column>();
    private List<Column> orders = new List<Column>();
    private Condition where;
    private Condition having;

    public string Comment { get; set; }

    /// <summary>
    /// FROM句の後ろに付与される。
    /// SelectをAdapterにセットした時Adapter.InitSelectEventをコールし、そこでセットされます。
    /// 返り値は追加する文字列。" FOR UPDATE"の様に頭にスペースを挿入してください。
    /// </summary>
    internal Func<Select, string> AfterFromFunc { get; set; }

    /// <summary>
    /// ORDER句の後ろに付与される。
    /// <seealso cref="AfterFromFunc"/>
    /// </summary>
    internal Func<Select, string> AfterOrderFunc { get; set; }

    /// <summary>
    /// コメントを付与します。SQLには影響しません。
    /// このコメントは<see cref="Log.Comment"/>から取得可能。
    /// つまり<see cref="Profiler"/>をONにしないと意味がありません。
    /// <see cref="Profiler"/>は<see cref="Sdx.Context.DbProfiler"/>にセットするとONになります。
    /// </summary>
    /// <param name="comment"></param>
    /// <returns></returns>
    public Select SetComment(string comment)
    {
      this.Comment = comment;
      return this;
    }

    internal Select(Adapter.Base adapter)
    {
      this.JoinOrder = JoinOrder.InnerFront;
      this.where = new Condition();
      this.having = new Condition();

      //intは0で初期化されてしまうのでセットされていない状態を識別するため（`LIMIT 0`を可能にするため）-1をセット
      this.Limit = -1;

      this.Adapter = adapter;
    }

    private Adapter.Base adapter;

    public Adapter.Base Adapter
    {
      get
      {
        return this.adapter;
      }

      internal set
      {
        this.adapter = value;
        this.adapter.InitSelectEvent(this);
      }
    }

    internal List<Column> GroupList
    {
      get { return this.groups; }
    }

    internal List<Column> OrderList
    {
      get { return this.orders; }
    }

    internal List<Column> ColumnList
    {
      get { return this.columns; }
    }

    /// <summary>
    /// デフォルトではINNER JOINを先に、LEFT JOINを後にします。
    /// <see cref="JoinOrder.Natural"/>をセットするとAddした順番にORDERされます。
    /// </summary>
    public JoinOrder JoinOrder { get; set; }

    public Select AddFrom(Sdx.Db.Table target, Action<Context> call)
    {
      call(AddFrom(target));
      return this;
    }

    public Select AddFrom(Sdx.Db.Table target, string alias, Action<Context> call)
    {
      call(AddFrom(target, alias));
      return this;
    }

    /// <summary>
    /// From句を追加。繰り返しコールすると繰り返し追加します。
    /// </summary>
    public Context AddFrom(Sdx.Db.Table target, string alias = null)
    {
      var context = this.CreateContext(target.OwnMeta.Name, alias, JoinType.From);
      context.Table = target;
      target.Context = context;
      target.AddAllColumnsFromMeta();
      return context;
    }

    public Select AddFrom(Expr target, Action<Context> call)
    {
      call(AddFrom(target));
      return this;
    }

    public Select AddFrom(Expr target, string alias, Action<Context> call)
    {
      call(AddFrom(target, alias));
      return this;
    }

    /// <summary>
    /// From句を追加。繰り返しコールすると繰り返し追加します。
    /// </summary>
    public Context AddFrom(Expr target, string alias = null)
    {
      return this.CreateContext(target, alias, JoinType.From);
    }

    public Select AddFrom(String target, Action<Context> call)
    {
      call(AddFrom(target));
      return this;
    }

    public Select AddFrom(String target, string alias, Action<Context> call)
    {
      call(AddFrom(target, alias));
      return this;
    }

    /// <summary>
    /// From句を追加。繰り返しコールすると繰り返し追加します。
    /// </summary>
    public Context AddFrom(String target, string alias = null)
    {
      return this.CreateContext(target, alias, JoinType.From);
    }

    public Select AddFrom(Sdx.Db.Sql.Select target, Action<Context> call)
    {
      call(AddFrom(target));
      return this;
    }

    public Select AddFrom(Sdx.Db.Sql.Select target, string alias, Action<Context> call)
    {
      call(AddFrom(target, alias));
      return this;
    }

    /// <summary>
    /// サブクエリーをJOINします。
    /// From句を追加。繰り返しコールすると繰り返し追加します。
    /// </summary>
    public Context AddFrom(Sdx.Db.Sql.Select target, string alias = null)
    {
      return this.CreateContext(target, alias, JoinType.From);
    }

    internal Context CreateContext(object target, string alias, JoinType jointType)
    {
      Context context = new Context(this);
      context.Alias = alias;
      context.JoinType = jointType;
      context.Target = target;

      this.RemoveContext(context.Name);

      this.contextList.Add(context.Name, context);

      return context;
    }

    internal string BuildSelectString(DbParameterCollection parameters, Counter condCount)
    {
      var builder = new StringBuilder();
      builder.Append("SELECT ");

      if (this.AppendColumnString(builder, parameters, condCount))
      {
        builder.Append(" ");
      }

      builder.Append("FROM ");

      //FROMを追加
      var hasFrom = false;
      this.contextList.ForEach((name, context) => {
        if(context.JoinType == JoinType.From)
        {
          hasFrom = true;
          this.BuildJoinString(builder, context, parameters, condCount);
          builder.Append(", ");
        }
      });

      if (hasFrom)
      {
        builder.Remove(builder.Length - 2, 2);
      }

      if (AfterFromFunc != null)
      {
        builder.Append(this.AfterFromFunc(this));
      }

      //JOIN
      if (this.JoinOrder == JoinOrder.InnerFront)
      {
        this.contextList.ForEach((name, context) => {
          if (context.JoinType == JoinType.Inner)
          {
            this.BuildJoinString(builder, context, parameters, condCount);
          }
        });

        this.contextList.ForEach((name, context) => {
          if (context.JoinType == JoinType.Left)
          {
            this.BuildJoinString(builder, context, parameters, condCount);
          }
        });
      }
      else
      {
        this.contextList.ForEach((name, context) => {
          if (context.JoinType == JoinType.Inner || context.JoinType == JoinType.Left)
          {
            this.BuildJoinString(builder, context, parameters, condCount);
          }
        });
      }

      if (this.Where.Count > 0)
      {
        builder.Append(" WHERE ");
        this.Where.Build(builder, this.Adapter, parameters, condCount);
      }

      //GROUP
      if (this.GroupList.Count > 0)
      {
        builder.Append(" GROUP BY ");
        this.GroupList.ForEach(column =>
        {
          builder
            .Append(column.Build(this.Adapter, parameters, condCount))
            .Append(", ");
        });

        builder.Remove(builder.Length - 2, 2);
      }

      //Having
      if (this.Having.Count > 0)
      {
        builder.Append(" HAVING ");
        this.Having.Build(builder, this.Adapter, parameters, condCount);
      }

      //ORDER
      if (this.OrderList.Count > 0)
      {
        builder.Append(" ORDER BY ");
        var orderString = "";
        this.OrderList.ForEach(column =>
        {
          if (orderString.Length > 0)
          {
            orderString += ", ";
          }

          builder
            .Append(column.Build(this.Adapter, parameters, condCount));

          if(column.Order != null)
          {
            var sqlstr = ((Order)column.Order).SqlString();
            builder
              .Append(" ")
              .Append(sqlstr);
          }

          builder.Append(", ");
        });

        builder.Remove(builder.Length - 2, 2);
      }


      if (AfterOrderFunc != null)
      {
        builder.Append(this.AfterOrderFunc(this));
      }

      return builder.ToString();
    }

    private void BuildJoinString(StringBuilder builder, Context context, DbParameterCollection parameters, Counter condCount)
    {
      if (context.JoinType != JoinType.From)
      {
        builder
          .Append(" ")
          .Append(context.JoinType.SqlString())
          .Append(" ");
      }

      if (context.Target is Select)
      {
        Select select = context.Target as Select;
        string subquery = select.BuildSelectString(parameters, condCount);
        builder
          .Append("(")
          .Append(subquery)
          .Append(")");
      }
      else
      {
        builder.Append(this.Adapter.QuoteIdentifier(context.Target));
      }

      if (context.Alias != null)
      {
        builder
          .Append(" AS ")
          .Append(this.Adapter.QuoteIdentifier(context.Name));
      }

      if (context.JoinCondition != null)
      {
        var jcBuilder = new StringBuilder();
        context.JoinCondition.Build(jcBuilder, this.Adapter, parameters, condCount);
        builder
          .Append(" ON ")
          .AppendFormat(
            jcBuilder.ToString(),
            this.Adapter.QuoteIdentifier(context.ParentContext.Name),
            this.Adapter.QuoteIdentifier(context.Name)
          );
      }
    }

    /// <summary>
    /// SELECT文を組み立てて<see cref="DbCommand"/>を返します。
    /// </summary>
    /// <returns></returns>
    public DbCommand Build()
    {
      if (this.Adapter == null)
      {
        throw new InvalidOperationException("Missing adapter, Set before Build.");
      }

      //Group Byに無いカラムは自動的にOrderから取り除かれます。
      //SELECT句はからは取り除きません。DBベンダーによっては取得できますし、意味がないわけではないので。
      if (groups.Count > 0)
      {
        orders = orders
          .Where(orderCol => groups.Any(groupCol => orderCol.SameAs(groupCol)))
          .ToList<Column>();
      }

      DbCommand command = this.Adapter.CreateCommand();

      var condCount = new Counter();
      command.CommandText = this.BuildSelectString(command.Parameters, condCount);

      if (this.Comment != null)
      {
        command.Parameters.Add(this.Adapter.CreateParameter(CommentParameterKey, this.Comment));
      }

      return command;
    }

    /// <summary>
    /// FROM句/JOIN句にテーブルが追加されているかチェックします。計算量はO(n)。
    /// </summary>
    /// <param name="contextName"></param>
    /// <returns></returns>
    public bool HasContext(string contextName)
    {
      return this.contextList.ContainsKey(contextName);
    }

    public IEnumerable<KeyValuePair<string, Context>> ContextList
    {
      get
      {
        return contextList;
      }
    }

    /// <summary>
    /// From及びJoinしたテーブルの中からcontextNameのテーブルを探し返す
    /// </summary>
    /// <param name="contextName"></param>
    /// <returns></returns>
    public Context Context(string contextName)
    {
      if (this.contextList.ContainsKey(contextName))
      {
        return this.contextList[contextName];
      }

      throw new InvalidOperationException("Missing " + contextName + " context.");
    }

    /// <summary>
    /// 追加したカラムをクリアする。
    /// </summary>
    /// <param contextName="context">contextNameを渡すとそのテーブルのカラムのみをクリアします。</param>
    /// <returns></returns>
    public Select ClearColumns(string contextName = null)
    {
      if (contextName == null)
      {
        this.columns.Clear();
      }
      else
      {
        this.columns.RemoveAll(column => column.ContextName != null && column.ContextName == contextName);
      }

      return this;
    }

    /// <summary>
    /// カラムを複数追加します。エイリアスの付与はできません。
    /// </summary>
    /// <param contextName="columns">Sdx.Adapter.Query.Expr[]|String[] 配列の中にExprを混ぜられるようにobjectなってます。</param>
    /// <returns></returns>
    public Select AddColumns(params object[] columns)
    {
      foreach (var column in columns)
      {
        this.columns.Add(new Column((dynamic)column));
      }
      return this;
    }

    public Select AddColumn(Select select, string alias = null)
    {
      this.columns.Add(new Column(select, null, alias));
      return this;
    }

    public Select AddColumn(Expr expr, string alias = null)
    {
      this.columns.Add(new Column(expr, null, alias));
      return this;
    }

    public Select AddColumn(string columnName, string alias = null)
    {
      this.columns.Add(new Column(columnName, null, alias));
      return this;
    }

    public Select SetColumns(params object[] columns)
    {
      this.ClearColumns();
      this.AddColumns(columns);
      return this;
    }

    public Select SetColumn(Select subquery, string alias = null)
    {
      this.ClearColumns();
      this.AddColumn(subquery, alias);
      return this;
    }

    public Select SetColumn(Expr expr, string alias = null)
    {
      this.ClearColumns();
      this.AddColumn(expr, alias);
      return this;
    }

    public Select SetColumn(string columnName, string alias = null)
    {
      this.ClearColumns();
      this.AddColumn(columnName, alias);
      return this;
    }

    internal bool AppendColumnString(StringBuilder builder, DbParameterCollection parameters, Counter condCount)
    {
      var hasColumn = false;
      this.ColumnList.ForEach((column) =>
      {
        hasColumn = true;
        builder
          .Append(column.Build(this.Adapter, parameters, condCount))
          .Append(", ");
      });

      if (hasColumn)
      {
        builder.Remove(builder.Length - 2, 2);
      }

      return hasColumn;
    }

    /// <summary>
    /// From及びJoinしたテーブルの中からcontextNameのテーブルを探し削除する
    /// </summary>
    /// <param contextName="contextName"></param>
    /// <returns></returns>
    public Select RemoveContext(string contextName)
    {
      if(this.contextList.ContainsKey(contextName))
      {
        var context = this.contextList[contextName];
        this.ClearColumns(contextName);
        this.contextList.Remove(contextName);
      }

      return this;
    }

    /// <summary>
    /// WHERE句の付与はこのプロパティー経由で行います。
    /// </summary>
    public Condition Where
    {
      get
      {
        this.where.ContextName = null;
        return this.where;
      }
    }

    public Select WhereCall(Action<Condition> callback)
    {
      callback.Invoke(Where);
      return this;
    }

    /// <summary>
    /// GROUP句を追加します。繰り返しコールすると繰り返し追加します。
    /// </summary>
    /// <param contextName="expr">Sdx.Adapter.Query.Expr|String</param>
    /// <returns></returns>
    public Select AddGroup(Expr expr)
    {
      var column = new Column(expr);
      this.groups.Add(column);
      return this;
    }

    public Select AddGroup(Select select)
    {
      var column = new Column(select);
      this.groups.Add(column);
      return this;
    }

    public Select AddGroup(string columnName)
    {
      var column = new Column(columnName);
      this.groups.Add(column);
      return this;
    }

    /// <summary>
    /// HAVING句はこのプロパティー経由で行います。
    /// </summary>
    public Condition Having
    {
      get
      {
        this.having.ContextName = null;
        return this.having;
      }
    }

    public int Limit { get; private set; }

    public int Offset { get; private set; }

    /// <summary>
    /// LIMIT句とOFFSET句を付与します。<see cref="Select"/>を返すのでFluentSyntaxが可能です。
    /// </summary>
    /// <param name="limit"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public Select SetLimit(int limit, int offset = 0)
    {
      this.Limit = limit;
      this.Offset = offset;
      return this;
    }


    /// <summary>
    /// ORDER句を追加します。繰り返しコールすると繰り返し追加します。
    /// </summary>
    /// <param contextName="expr">Sdx.Adapter.Query.Expr|String</param>
    /// <param contextName="order"></param>
    /// <returns></returns>
    public Select AddOrder(Expr expr, Order order)
    {
      var column = new Column(expr);
      column.Order = order;
      orders.Add(column);

      return this;
    }

    public Select AddOrder(Select select, Order order)
    {
      var column = new Column(select);
      column.Order = order;
      orders.Add(column);

      return this;
    }

    public Select AddOrder(string columnName, Order order)
    {
      var column = new Column(columnName);
      column.Order = order;
      orders.Add(column);

      return this;
    }

    public Select AddOrderRandom()
    {
      var column = new Column(Expr.Wrap(Adapter.RandomOrderKeyword));
      column.Order = null;
      orders.Add(column);

      return this;
    }

    public object Clone()
    {
      var cloned = (Select)this.MemberwiseClone();

      //context
      cloned.contextList = new Collection.OrderedDictionary<string, Sql.Context>();
      this.contextList.ForEach((name, context) => {
        var clonedContext = (Context)context.Clone();
        clonedContext.Select = cloned;

        if (context.ParentContext != null)
        {
          clonedContext.ParentContext = cloned.Context(context.ParentContext.Name);
        }

        cloned.contextList.Add(clonedContext.Name, clonedContext);
      });

      //コピーコンストラクタはシャローコピーです。
      cloned.columns = new List<Column>(this.columns);
      cloned.groups = new List<Column>(this.groups);
      cloned.orders = new List<Column>(this.orders);

      //where
      cloned.where = (Condition)this.where.Clone();

      //having
      cloned.having = (Condition)this.having.Clone();

      return cloned;
    }

    public bool ForUpdate { get; set; }


    public void LimitPage(int page, int perPage)
    {
      Offset = perPage * (page - 1);
      Limit = perPage;
    }

    public void LimitPager(Pager Pager)
    {
      LimitPage(Pager.Page, Pager.PerPage);
    }

    public Connection Connection { get; internal set; }

    public ContextActions CreateContextActions()
    {
      return new ContextActions(this);
    }
  }
}
