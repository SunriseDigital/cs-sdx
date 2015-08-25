using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Sdx.Db.Query
{
  public class Select
  {
    private Adapter adapter;
    private List<Context> contextList = new List<Context>();
    private List<Column> columns = new List<Column>();
    private List<Column> groups = new List<Column>();
    private List<Column> orders = new List<Column>();
    private Condition where;
    private Condition having;

    internal Select(Adapter adapter)
    {
      this.adapter = adapter;
      this.JoinOrder = JoinOrder.InnerFront;
      this.where = new Condition();
      this.having = new Condition();

      //intは0で初期化されてしまうのでセットされていない状態を識別するため（`LIMIT 0`を可能にするため）-1をセット
      this.Limit = -1;
    }

    internal Adapter Adapter
    {
      get { return this.adapter; }
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
      target.ContextName = context.Name;
      target.Select = this;
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

    public Context AddFrom(Sdx.Db.Query.Select target, string alias = null)
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
      string selectString = "SELECT";

      var columnString = this.BuildColumsString();
      if (columnString.Length > 0)
      {
        selectString += " " + columnString;
      }
      selectString += " FROM ";

      //FROMを追加
      var fromString = "";
      foreach (Context context in this.contextList.Where(t => t.JoinType == JoinType.From))
      {
        if (fromString != "")
        {
          fromString += ", ";
        }
        fromString += this.BuildJoinString(context, parameters, condCount);
      }

      selectString += fromString;

      //JOIN
      if (this.JoinOrder == JoinOrder.InnerFront)
      {
        foreach (var context in this.contextList.Where(t => t.JoinType == JoinType.Inner))
        {
          selectString += this.BuildJoinString(context, parameters, condCount);
        }

        foreach (var context in this.contextList.Where(t => t.JoinType == JoinType.Left))
        {
          selectString += this.BuildJoinString(context, parameters, condCount);
        }
      }
      else
      {
        foreach (var context in this.contextList.Where(t => t.JoinType == JoinType.Inner || t.JoinType == JoinType.Left))
        {
          selectString += this.BuildJoinString(context, parameters, condCount);
        }
      }

      if (this.where.Count > 0)
      {
        selectString += " WHERE ";
        selectString += this.where.Build(this, parameters, condCount);
      }

      //GROUP
      if (this.GroupList.Count > 0)
      {
        var groupString = "";
        this.GroupList.ForEach(column =>
        {
          if(groupString != "")
          {
            groupString += ", ";
          }

          groupString += column.Build(this.adapter);
        });

        selectString += " GROUP BY " + groupString;
      }

      //Having
      if(this.having.Count > 0)
      {
        selectString += " HAVING ";
        selectString += this.having.Build(this, parameters, condCount);
      }

      //ORDER
      if(this.orders.Count > 0)
      {
        var orderString = "";
        this.orders.ForEach(column => { 
          if(orderString.Length > 0)
          {
            orderString += ", ";
          }
          orderString += column.Build(this.adapter) + " " + column.Order.SqlString();
        });

        selectString += " ORDER BY " + orderString;
      }

      //LIMIT/OFFSET
      if(this.Limit > -1)
      {
        selectString = this.adapter.AppendLimitQuery(selectString, this.Limit, this.Offset);
      }

      return selectString;
    }

    private string BuildJoinString(Context context, DbParameterCollection parameters, Counter condCount)
    {
      string joinString = "";

      if (context.JoinType != JoinType.From)
      {
        joinString += " " + context.JoinType.SqlString() + " ";
      }

      if (context.Target is Select)
      {
        Select select = context.Target as Select;
        string subquery = select.BuildSelectString(parameters, condCount);
        joinString += "(" + subquery + ")";
      }
      else
      {
        joinString += this.Adapter.QuoteIdentifier(context.Target);
      }

      if (context.Alias != null)
      {
        joinString += " AS " + this.Adapter.QuoteIdentifier(context.Name);
      }

      if (context.JoinCondition != null)
      {
        joinString += " ON "
          + String.Format(
            context.JoinCondition.Build(this, parameters, condCount),
            this.Adapter.QuoteIdentifier(context.ParentContext.Name),
            this.Adapter.QuoteIdentifier(context.Name)
          );
      }

      return joinString;
    }

    public DbCommand Build()
    {
      DbCommand command = this.adapter.CreateCommand();
      var condCount = new Counter();
      command.CommandText = this.BuildSelectString(command.Parameters, condCount);

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

      throw new Exception("Missing " + contextName + " context.");
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

    internal string BuildColumsString()
    {
      var result = "";
      this.columns.ForEach((column) =>
      {
        if (result.Length > 0)
        {
          result += ", ";
        }

        result += column.Build(this.adapter);
      });

      return result;
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

    private int Limit { get; set; }

    private int Offset { get; set; }

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
  }
}
