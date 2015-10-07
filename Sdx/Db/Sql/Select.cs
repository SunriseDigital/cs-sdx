using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Sdx.Db.Sql
{
  public class Select : ICloneable
  {
    public const string CommentParameterKey = "##Sdx.Db.Query.Select.Comment##";

    private List<Context> contextList = new List<Context>();
    private List<Column> columns = new List<Column>();
    private List<Column> groups = new List<Column>();
    private List<Column> orders = new List<Column>();
    private Condition where;
    private Condition having;

    public string Comment { get; set; }
    internal Func<Select, string> AfterFromFunc { get; set; }
    internal Func<Select, string> AfterOrderFunc { get; set; }

    public Select SetComment(string comment)
    {
      this.Comment = comment;
      return this;
    }

    internal Select(Adapter adapter)
    {
      this.JoinOrder = JoinOrder.InnerFront;
      this.where = new Condition();
      this.having = new Condition();

      //intは0で初期化されてしまうのでセットされていない状態を識別するため（`LIMIT 0`を可能にするため）-1をセット
      this.Limit = -1;

      this.Adapter = adapter;
    }

    private Adapter adapter;

    public Adapter Adapter
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

    internal List<Context> ContextList
    {
      get { return this.contextList; }
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

    public JoinOrder JoinOrder { get; set; }

    /// <summary>
    /// From句を追加。繰り返しコールすると繰り返し追加します。
    /// </summary>
    public Context AddFrom(Sdx.Db.Table target, string alias = null)
    {
      var context = this.CreateContext(target.OwnMeta.Name, alias);
      context.Table = target;
      target.Context = context;
      target.AddAllColumnsFromMeta();
      return context;
    }

    /// <summary>
    /// From句を追加。繰り返しコールすると繰り返し追加します。
    /// </summary>
    public Context AddFrom(Expr target, string alias = null)
    {
      return this.CreateContext(target, alias);
    }

    /// <summary>
    /// From句を追加。繰り返しコールすると繰り返し追加します。
    /// </summary>
    public Context AddFrom(String target, string alias = null)
    {
      return this.CreateContext(target, alias);
    }

    public Context AddFrom(Sdx.Db.Sql.Select target, string alias = null)
    {
      return this.CreateContext(target, alias);
    }

    private Context CreateContext(object target, string alias)
    {
      Context context = new Context(this);
      context.Alias = alias;
      context.JoinType = JoinType.From;
      context.Target = target;
      this.contextList.Add(context);

      return context;
    }

    internal string BuildSelectString(DbParameterCollection parameters, Counter condCount)
    {
      var builder = new StringBuilder();
      builder.Append("SELECT ");

      if (this.AppendColumnString(builder))
      {
        builder.Append(" ");
      }

      builder.Append("FROM ");

      //FROMを追加
      var hasFrom = false;
      foreach (Context context in this.ContextList.Where(t => t.JoinType == JoinType.From))
      {
        hasFrom = true;
        this.BuildJoinString(builder, context, parameters, condCount);
        builder.Append(", ");
      }

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
        foreach (var context in this.ContextList.Where(t => t.JoinType == JoinType.Inner))
        {
          this.BuildJoinString(builder, context, parameters, condCount);
        }

        foreach (var context in this.ContextList.Where(t => t.JoinType == JoinType.Left))
        {
          this.BuildJoinString(builder, context, parameters, condCount);
        }
      }
      else
      {
        foreach (var context in this.ContextList.Where(t => t.JoinType == JoinType.Inner || t.JoinType == JoinType.Left))
        {
          this.BuildJoinString(builder, context, parameters, condCount);
        }
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
            .Append(column.Build(this.Adapter))
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
            .Append(column.Build(this.Adapter))
            .Append(" ")
            .Append(column.Order.SqlString())
            .Append(", ");
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

    public DbCommand Build()
    {
      if (this.Adapter == null)
      {
        throw new InvalidOperationException("Missing adapter, Set before Build.");
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

    public bool HasContext(string contextName)
    {
      int findIndex = this.contextList.FindIndex(context =>
      {
        return context.Name == contextName;
      });

      return findIndex != -1;
    }


    /// <summary>
    /// From及びJoinしたテーブルの中からcontextNameのテーブルを探し返す
    /// </summary>
    /// <param name="contextName"></param>
    /// <returns></returns>
    public Context Context(string contextName)
    {
      foreach (Context context in this.contextList)
      {
        if (context.Name == contextName)
        {
          return context;
        }
      }

      throw new InvalidOperationException("Missing " + contextName + " context.");
    }

    /// <summary>
    /// 追加したカラムをクリアする。
    /// </summary>
    /// <param contextName="context">Contextを渡すとそのテーブルのカラムのみをクリアします。</param>
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
    /// エイリアスの付与はできません。
    /// </summary>
    /// <param contextName="columns">Sdx.Adapter.Query.Expr[]|String[] 配列の中にExprを混ぜられるようにobjectなってます。</param>
    /// <returns></returns>
    public Select AddColumns(params object[] columns)
    {
      foreach (var column in columns)
      {
        this.AddColumn(column);
      }
      return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param contextName="columnName">Sdx.Adapter.Query.Expr|String</param>
    /// <param contextName="alias"></param>
    /// <returns></returns>
    public Select AddColumn(object columnName, string alias = null)
    {
      var column = new Column(columnName);
      column.Alias = alias;
      this.columns.Add(column);
      return this;
    }

    internal bool AppendColumnString(StringBuilder builder)
    {
      var hasColumn = false;
      this.ColumnList.ForEach((column) =>
      {
        hasColumn = true;
        builder
          .Append(column.Build(this.Adapter))
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
      int findIndex = this.contextList.FindIndex(jt =>
      {
        return jt.Name == contextName;
      });

      if (findIndex != -1)
      {
        this.ClearColumns(this.contextList[findIndex].Name);
        this.contextList.RemoveAt(findIndex);
      }

      return this;
    }

    public Condition Where
    {
      get
      {
        this.where.Context = null;
        return this.where;
      }
    }

    /// <summary>
    /// 繰り返しコールすると繰り返し追加します。
    /// </summary>
    /// <param contextName="columnName">Sdx.Adapter.Query.Expr|String</param>
    /// <returns></returns>
    public Select AddGroup(object columnName)
    {
      var column = new Column(columnName);
      this.groups.Add(column);
      return this;
    }

    public Condition Having
    {
      get
      {
        this.having.Context = null;
        return this.having;
      }
    }

    public int Limit { get; private set; }

    public int Offset { get; private set; }

    public Select SetLimit(int limit, int offset = 0)
    {
      this.Limit = limit;
      this.Offset = offset;
      return this;
    }


    /// <summary>
    /// 繰り返しコールすると繰り返し追加します。
    /// </summary>
    /// <param contextName="columnName">Sdx.Adapter.Query.Expr|String</param>
    /// <param contextName="order"></param>
    /// <returns></returns>
    public Select AddOrder(object columnName, Order order)
    {
      var column = new Column(columnName);
      column.Order = order;
      orders.Add(column);

      return this;
    }

    public object Clone()
    {
      var cloned = (Select)this.MemberwiseClone();

      //context
      cloned.contextList = new List<Sql.Context>();
      this.contextList.ForEach(context =>
      {
        var clonedContext = (Context)context.Clone();
        clonedContext.Select = cloned;
        cloned.contextList.Add(clonedContext);
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
  }
}
