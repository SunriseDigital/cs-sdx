using System;
using System.Collections.Generic;

namespace Sdx.Db
{
  /// <summary>
  /// テーブルの名前や保持するカラム、他テーブルとの関係といったテーブルの情報を保持するクラス。
  /// 各Tableクラスのstaticなプロパティ、Metaに保持され、普遍なので複数生成する必要のないクラス。
  /// </summary>
  public class MetaData
  {
    public MetaData(
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
    }

    public string Name { get; private set; }
    public List<string> Pkeys { get; private set; }
    public List<Table.Column> Columns { get; private set; }
    public Dictionary<string, Sdx.Db.Table.Relation> Relations { get; private set; }
    public Type TableType { get; private set; }
    public Type RecordType { get; private set; }

    public Query.Condition CreateJoinCondition(string contextName)
    {
      var cond = new Query.Condition();

      if(!this.Relations.ContainsKey(contextName))
      {
        throw new KeyNotFoundException("Missing " + contextName + " relation in " + this.TableType);
      }

      var relation = this.Relations[contextName];
      cond.Add(
        new Query.Column(relation.ForeignKey, this.Name),
        new Query.Column(relation.ReferenceKey, contextName)
      );

      return cond;
    }
  }
}
