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
  public class Update
  {
    private List<string> keys = new List<string>();
    private List<object> values = new List<object>();

    public Adapter Adapter { get; private set; }
    public string Table { get; set; }
    public Condition Where { get; private set; } = new Condition();

    public Update(Adapter adapter)
    {
      this.Adapter = adapter;
    }

    public Update SetTable(string table)
    {
      this.Table = table;
      return this;
    }

    public Update AddPair(string column, object value)
    {
      this.keys.Add(column);
      this.values.Add(value);
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
      var index = 0;
      this.keys.ForEach(column => {
        builder
          .Append(this.Adapter.QuoteIdentifier(column))
          .Append(" = ");

        var value = this.values[index];

        if(value is Select)
        {
          var select = value as Select;
          builder
            .Append('(')
            .Append(select.BuildSelectString(command.Parameters, counter))
            .Append(')');
        }
        else if (value is Expr)
        {
          builder.Append(value.ToString());
        }
        else
        {
          var placeHolder = "@" + counter.Value;
          builder.Append(placeHolder);

          var param = command.CreateParameter();
          param.ParameterName = placeHolder;
          param.Value = value;
          command.Parameters.Add(param);

          counter.Incr();
        }

        if(index < this.keys.Count - 1)
        {
          builder.Append(", ");
        }

        ++index;
      });

      if (this.Where.Count > 0)
      {
        builder
          .Append(" WHERE ")
          .Append(this.Where.Build(this.Adapter, command.Parameters, counter));
      }

      command.CommandText = builder.ToString();
      return command;
    }
  }
}