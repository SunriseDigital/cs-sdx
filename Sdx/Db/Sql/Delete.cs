using System;
using System.Data.Common;
using System.Text;

namespace Sdx.Db.Sql
{
  public class Delete : INonQueryBuilder
  {
    public Adapter.Base Adapter { get; private set; }
    public string From { get; set; }
    public Condition Where { get; private set; }

    internal Delete(Adapter.Base adapter)
    {
      this.Adapter = adapter;
      this.Where = new Condition();
    }

    public Delete SetFrom(string table)
    {
      this.From = table;
      return this;
    }

    public DbCommand Build()
    {
      var command = this.Adapter.CreateCommand();

      var builder = new StringBuilder();
      builder
        .Append("DELETE FROM ")
        .Append(this.Adapter.QuoteIdentifier(this.From))
        ;

      //SET句
      var counter = new Counter();
      
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
      var cloned = (Delete)this.MemberwiseClone();

      cloned.Where = (Condition)this.Where.Clone();

      return cloned;
    }
  }
}