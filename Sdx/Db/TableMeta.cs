using System;
using System.Collections.Generic;

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
      List<string> pkeys,
      List<Table.Column> columns,
      Dictionary<string, Sdx.Db.Table.Relation> relations,
      Type recordType,
      Type tableType
    ){
      this.Name = name;
      this.Pkeys = pkeys;
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

      this.Columns.ForEach(column => {
        this.columnsCache[column.Name] = column;
      });
    }

    public string Name { get; private set; }
    public List<string> Pkeys { get; private set; }
    public List<Table.Column> Columns { get; private set; }
    public Dictionary<string, Sdx.Db.Table.Relation> Relations { get; private set; }
    public Type TableType { get; private set; }
    public Type RecordType { get; private set; }

    private Dictionary<string, Table.Column> columnsCache = new Dictionary<string, Table.Column>();

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

    public Sql.Condition CreateJoinCondition(string contextName)
    {
      var cond = new Sql.Condition();

      if(!this.Relations.ContainsKey(contextName))
      {
        throw new KeyNotFoundException("Missing " + contextName + " relation in " + this.TableType);
      }

      var relation = this.Relations[contextName];
      cond.Add(
        new Sql.Column(relation.ForeignKey, this.Name),
        new Sql.Column(relation.ReferenceKey, contextName)
      );

      return cond;
    }
  }
}
