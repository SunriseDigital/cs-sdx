using System;
using System.Collections.Generic;
using System.Linq;

namespace Sdx.Db
{
  /// <summary>
  /// テーブルの名前や保持するカラム、他テーブルとの関係といったテーブルの情報を保持するクラス。
  /// 各Tableクラスのstaticなプロパティ、Metaに保持され、普遍なので複数生成する必要のないクラス。
  /// </summary>
  public class TableMeta
  {
    public TableMeta(
      string name,
      List<Table.Column> columns,
      Dictionary<string, Sdx.Db.Table.Relation> relations,
      Type recordType,
      Type tableType
    ){
      this.Name = name;
      InitializeTableMeta(columns, relations, recordType, tableType);
    }

    public TableMeta(
      Func<string> nameGetter,
      List<Table.Column> columns,
      Dictionary<string, Sdx.Db.Table.Relation> relations,
      Type recordType,
      Type tableType
    )
    {
      this.NameGetter = nameGetter;
      InitializeTableMeta(columns, relations, recordType, tableType);
    }

    private void InitializeTableMeta(
      List<Table.Column> columns,
      Dictionary<string, Sdx.Db.Table.Relation> relations,
      Type recordType,
      Type tableType)
    {
      this.Columns = columns;
      this.Relations = relations;

      if (!typeof(Sdx.Db.Record).IsAssignableFrom(recordType))
      {
        throw new NotSupportedException(recordType + " is not Sdx.Db.Record subclass");
      }
      this.RecordType = recordType;

      if (!typeof(Sdx.Db.Table).IsAssignableFrom(tableType))
      {
        throw new NotSupportedException(tableType + "is not Sdx.Db.Table subclass");
      }
      this.TableType = tableType;

      this.Columns.ForEach(column =>
      {
        this.columnsCache[column.Name] = column;
        column.Meta = this;
      });
    }

    private string name;
    public string Name
    {
      get
      {
        if (this.name != null)
        {
          return this.name;
        }
        else
        {
          return this.NameGetter();
        }
      }

      private set 
      { 
        this.name = value; 
      }
    }
    public List<Table.Column> Columns { get; private set; }
    public Dictionary<string, Sdx.Db.Table.Relation> Relations { get; private set; }
    public Type TableType { get; private set; }
    public Type RecordType { get; private set; }
    public Func<string> NameGetter { get; private set; }

    private Dictionary<string, Table.Column> columnsCache = new Dictionary<string, Table.Column>();

    private List<Table.Column> pkeys;

    public IEnumerable<Table.Column> Pkeys
    {
      get
      {
        if(pkeys == null)
        {
          pkeys = Columns.Where((col) => col.IsPkey).ToList();
        }

        return pkeys;
      }
    }

    public Table CreateTable()
    {
      return (Table)this.TableType.GetConstructor(Type.EmptyTypes).Invoke(null);
    }

    public Record CreateRecord()
    {
      return (Record)this.RecordType.GetConstructor(Type.EmptyTypes).Invoke(null);
    }

    public bool HasColumn(string columnName)
    {
      return this.columnsCache.ContainsKey(columnName);
    }

    public void CheckColumn(string columnName)
    {
      if (!this.HasColumn(columnName))
      {
        throw new KeyNotFoundException("Missing " + columnName + " in " + this.TableType);
      }
    }

    public Table.Column GetColumn(string columnName)
    {
      this.CheckColumn(columnName);
      return this.columnsCache[columnName];
    }

    public Sql.Condition CreateJoinCondition(string tableName, string alias = null)
    {
      if(alias == null)
      {
        alias = tableName;
      }

      var cond = new Sql.Condition();

      Table.Relation relation;
      if (this.Relations.ContainsKey(alias))
      {
        relation = this.Relations[alias];
      }
      else if (this.Relations.ContainsKey(tableName))
      {
        relation = this.Relations[tableName];
      }
      else
      {
        throw new KeyNotFoundException("Missing " + alias + " relation in " + this.TableType);
      }

      cond.Add(
        new Sql.Column(relation.ForeignKey, this.Name),
        new Sql.Column(relation.ReferenceKey, alias)
      );

      return cond;
    }
  }
}
