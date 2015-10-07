using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Sdx.Db.Sql
{
  /// <summary>
  /// Update文を生成するクラス。
  /// MySQLとSQLServerとでサブクエリを使った複数のカラムのアップデートのSQL文法がかなり違うため
  /// また、使用頻度も低いと思われるので対応していません。
  /// 必要な場合はDbCommandを直接生成してください。
  /// Where句のサブクエリおよび、単一カラムのサブクエリーによる値の指定には対応しています。
  /// </summary>
  public class Update : INonQueryBuilder
  {
    private List<Column> columns = new List<Column>();

    public Adapter Adapter { get; private set; }
    public string Table { get; set; }
    public Condition Where { get; private set; } = new Condition();

    internal Update(Adapter adapter)
    {
      this.Adapter = adapter;
    }

    public Update SetTable(string table)
    {
      this.Table = table;
      return this;
    }

    public Update AddColumnValue(string columnName, object value)
    {
      var column = new Column(columnName);
      column.Value = value;
      this.columns.Add(column);
      return this;
    }

    public DbCommand Build()
    {
      var command = this.Adapter.CreateCommand();

      var builder = new StringBuilder();
      builder
        .Append("UPDATE ")
        .Append(this.Adapter.QuoteIdentifier(this.Table))
        .Append(" SET ");

      //SET句
      var counter = new Counter();
      this.columns.ForEach(column => {
        builder
          .Append(column.Build(this.Adapter))
          .Append(" = ");


        if(column.Value is Select)
        {
          var select = column.Value as Select;
          builder
            .Append('(')
            .Append(select.BuildSelectString(command.Parameters, counter))
            .Append(')');
        }
        else if (column.Value is Expr)
        {
          builder.Append(column.Value.ToString());
        }
        else
        {
          var placeHolder = "@" + counter.Value;
          builder.Append(placeHolder);

          var param = command.CreateParameter();
          param.ParameterName = placeHolder;
          param.Value = column.Value;
          command.Parameters.Add(param);

          counter.Incr();
        }

        builder.Append(", ");
      });

      builder.Remove(builder.Length - 2, 2);

      if (this.Where.Count > 0)
      {
        builder.Append(" WHERE ");
        this.Where.Build(builder, this.Adapter, command.Parameters, counter);
      }

      command.CommandText = builder.ToString();
      return command;
    }

    public object Clone()
    {
      var cloned = (Update)this.MemberwiseClone();

      cloned.columns = new List<Column>(this.columns);
      cloned.Where = (Condition)this.Where.Clone();

      return cloned;
    }
  }
}