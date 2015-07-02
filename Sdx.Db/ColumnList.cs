using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Db
{
  public class ColumnList
  {
    private List<String> columns;

    public ColumnList()
    {
      this.columns = new List<string>();
    }

    public int Count
    {
      get { return this.columns.Count; }
    }

    public ColumnList Add(params string[] columns)
    {
      foreach (string column in columns)
      {
        this.columns.Add(column);
      }

      return this;
    }

    public ColumnList Clear()
    {
      this.columns.Clear();

      return this;
    }

    public void ForEach(Action<String> action)
    {
      this.columns.ForEach(action);
    }
  }
}
