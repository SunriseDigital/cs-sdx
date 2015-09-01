using System;
using System.Collections.Generic;

namespace Sdx.Db
{
  /// <summary>
  /// SelectにおいてTableを表現するオブジェクト。メタ情報としてカラムや他テーブルとの関連情報を保持し、SELECTにセットする。
  /// またテーブルに付与されたエイリアス名を保持し、Recordに必要なカラム名のエイリアス生成も行う。
  /// </summary>
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

    public class Column
    {
      public Column(string name)
      {
        this.Name = name;
      }

      public string Name { get; private set; }
    }

    public Adapter Adapter { get; set; }
    private Query.Select select;

    public MetaData OwnMeta
    {
      get
      {
        var prop = this.GetType().GetProperty("Meta");
        if (prop == null)
        {
          throw new NotImplementedException("Missing Meta property in " + this.GetType());
        }

        var meta = prop.GetValue(null, null) as MetaData;
        if (meta == null)
        {
          throw new NotImplementedException("Initialize TableMeta for " + this.GetType());
        }

        return meta;
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
        throw new InvalidOperationException("ContextName is null");
      }

      alias = Record.BuildColumnAliasWithContextName(alias != null ? alias : columnName.ToString(), this.ContextName);
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
        this.OwnMeta.Columns.ForEach(column =>
        {
          this.AddColumn(column.Name);
        });
      }
    }
  }
}
