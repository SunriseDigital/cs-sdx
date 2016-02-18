﻿using System;
using System.Collections.Generic;

namespace Sdx.Db
{
  /// <summary>
  /// SelectにおいてTableを表現するオブジェクト。メタ情報としてカラムや他テーブルとの関連情報を保持し、SELECTにセットする。
  /// またテーブルに付与されたエイリアス名を保持し、Recordに必要なカラム名のエイリアス生成も行う。
  /// </summary>
  public abstract class Table : ICloneable
  {
    public class Relation
    {
      public Relation(Type tableType, string foreignKey, string referenceKey)
      {
        this.TableType = tableType;
        this.ForeignKey = foreignKey;
        this.ReferenceKey = referenceKey;
      }

      public Relation(Type tableType, string foreignKey) : this(tableType, foreignKey, foreignKey)
      {

      }

      public TableMeta TableMeta
      {
        get
        {
          var prop = TableType.GetProperty("Meta");
          if (prop == null)
          {
            throw new NotImplementedException("Missing Meta property in " + this.GetType());
          }

          var meta = prop.GetValue(null, null) as TableMeta;
          if (meta == null)
          {
            throw new NotImplementedException("Initialize TableMeta for " + this.GetType());
          }

          return meta;
        }
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

    public class Column
    {
      //TODO TypeはValidationの自動設定に使う予定。独自Typeに変更したほうがいいと思われる。
      public Column(string name, Type type = null, bool isNotNull = true, bool isAutoIncrement = false)
      {
        this.Name = name;
        this.Type = type;
        this.IsNotNull = isNotNull;
        this.IsAutoIncrement = isAutoIncrement;
      }

      public string Name { get; private set; }
      public Type Type { get; private set; }
      public bool IsNotNull {get; private set;}
      public bool IsAutoIncrement { get; private set; }

      public TableMeta Meta { get; internal set; }

      public bool IsPkey
      {
        get
        {
          return Meta.Pkeys.Exists((column) => this.Name == column);
        }
      }
    }

    public Adapter.Base Adapter { get; set; }

    public TableMeta OwnMeta
    {
      get
      {
        var prop = this.GetType().GetProperty("Meta");
        if (prop == null)
        {
          throw new NotImplementedException("Missing Meta property in " + this.GetType());
        }

        var meta = prop.GetValue(null, null) as TableMeta;
        if (meta == null)
        {
          throw new NotImplementedException("Initialize TableMeta for " + this.GetType());
        }

        return meta;
      }
    }

    public Table ClearColumns()
    {
      this.Select.ClearColumns(this.Context.Name);
      return this;
    }

    public Table AddColumn(object columnName, string alias = null)
    {
      if (this.Context == null)
      {
        throw new InvalidOperationException("ContextName is null");
      }

      alias = Record.BuildColumnAliasWithContextName(alias != null ? alias : columnName.ToString(), this.Context.Name);
      this.Select.Context(this.Context.Name).AddColumn(columnName, alias);

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

    public Sql.Context Context
    {
      get; set;
    }

    internal void AddAllColumnsFromMeta()
    {
      this.OwnMeta.Columns.ForEach(column =>
      {

        this.AddColumn(column.Name);
      });
    }

    /// <summary>
    /// テーブル経由でカラムを変更したとき、カラムの並び順が呼び出し順になってるのが自然だと思ったので、Table内でSelectを保持するようになっています。
    /// </summary>
    public Sql.Select Select
    {
      get
      {
        return this.Context.Select;
      }
    }

    public object Clone()
    {
      return this.MemberwiseClone();
    }

    public virtual Table SelectDefaultOrder(Sql.Select select)
    {
      return this;
    }
  }
}
