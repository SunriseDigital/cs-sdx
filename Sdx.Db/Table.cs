﻿using System;
using System.Collections.Generic;

namespace Sdx.Db
{
  public abstract class Table
  {
    public class Relation
    {
      public Relation(Type tableType, string foreignKey, string referenceKey)
      {
        this.TableType = tableType;
        this.ForeignKey = foreignKey;
        this.ReferenceKey = referenceKey;
      }

      public Type TableType { get; private set; }
      public string ForeignKey { get; private set; }
      public string ReferenceKey { get; private set; }
      public string JoinCondition
      {
        get
        {
          return "{0}." + this.ForeignKey + " = {1}." + this.ReferenceKey;
        }
      }
    }

    public Adapter Adapter { get; set; }
    private Query.Select select;

    public TableMeta TableMeta
    {
      get
      {
        var prop = this.GetType().GetProperty("Meta");
        if (prop == null)
        {
          throw new Exception("You must declare Meta property");
        }

        var tableMeta = prop.GetValue(null, null) as TableMeta;

        if(tableMeta == null)
        {
          throw new Exception("You must initialize Table Meta");
        }

        return tableMeta;
      }
    }

    public Table ClearColumns()
    {
      this.select.ClearColumns(this.ContextName);
      return this;
    }

    public Table AddColumn(object columnName, string alias = null)
    {
      if (this.ContextName == null)
      {
        throw new Exception("ContextName is null");
      }

      alias = alias != null ? alias + "@" + this.ContextName : columnName + "@" + this.ContextName;
      this.select.Context(this.ContextName).AddColumn(columnName, alias);

      return this;
    }

    public Table SetColumns(params object[] columns)
    {
      this.ClearColumns();
      this.AddColumns(columns);
      return this;
    }

    public Table AddColumns(params object[] columns)
    {
      foreach (var columnName in columns)
      {
        this.AddColumn(columnName);
      }
      return this;
    }

    internal string ContextName { get; set; }

    /// <summary>
    /// テーブル経由でカラムを変更したとき、カラムの並び順が呼び出し順になってるのが自然だと思ったので、Table内でSelectを保持するようになっています。
    /// </summary>
    internal Query.Select Select
    {
      get
      {
        return this.select;
      }
      set
      {
        this.select = value;
        this.TableMeta.Columns.ForEach(columnName =>
        {
          this.AddColumn(columnName);
        });
      }
    }
  }
}
