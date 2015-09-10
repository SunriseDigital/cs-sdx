using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Sdx.Db.Query
{
  public class Select : ICloneable
  {
    private List<Context> contextList = new List<Context>();
    private List<Column> columns = new List<Column>();
    private List<Column> groups = new List<Column>();
    private List<Column> orders = new List<Column>();

    public string Comment { get; set; }

    public Select SetComment(string comment)
    {
      this.Comment = comment;
      return this;
    }

    private Condition where;
    private Condition having;

    public Select()
    {
      this.JoinOrder = JoinOrder.InnerFront;
      this.where = new Condition();
      this.having = new Condition();

      //intは0で初期化されてしまうのでセットされていない状態を識別するため（`LIMIT 0`を可能にするため）-1をセット
      this.Limit = -1;
    }

    public Select(Adapter adapter) : this()
    {
      this.Adapter = adapter;
    }

    public Adapter Adapter { get; set; }

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

          groupString += column.Build(this.Adapter);
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
          orderString += column.Build(this.Adapter) + " " + column.Order.SqlString();
        });

        selectString += " ORDER BY " + orderString;
      }

      //LIMIT/OFFSET
      if(this.Limit > -1)
      {
        selectString = this.Adapter.AppendLimitQuery(selectString, this.Limit, this.Offset);
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
        if (select.Adapter == null)
        {
          select.Adapter = this.Adapter;
        }
        
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

    public const string CommentParameterKey = "##Sdx.Db.Query.Select.Comment##";

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

    internal string BuildColumsString()
    {
      var result = "";
      this.columns.ForEach((column) =>
      {
        if (result.Length > 0)
        {
          result += ", ";
        }

        result += column.Build(this.Adapter);
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

    public object Clone()
    {
      var cloned = (Select)this.MemberwiseClone();

      //context
      cloned.contextList = new List<Query.Context>();
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
  }
}
