using System.Data.Common;
using System.Text;

namespace Sdx.Db.Sql
{
  public class Delete
  {
    public Adapter Adapter { get; private set; }
    public string From { get; set; }
    public Condition Where { get; private set; } = new Condition();

    public Delete(Adapter adapter)
    {
      this.Adapter = adapter;
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
        builder
          .Append(" WHERE ")
          .Append(this.Where.Build(this.Adapter, command.Parameters, counter));
      }

      command.CommandText = builder.ToString();
      return command;
    }
  }
}