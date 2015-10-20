# Sdx.Db.Sql.Update

## 概要

UPDATE文を生成します。

## 使い方

```c#
var db = new Sdx.Db.SqlServerAdapter();
db.ConnectionString = "**接続文字列**";

var update = db.CreateUpdate();

update
  .SetTable("shop")
  .AddColumnValue("name", "NewName")
  .AddColumnValue("area_id", 3)
  .Where.Add("id", 2);

using (var conn = db.CreateConnection())
{
  conn.Open();
  conn.BeginTransaction();
  conn.Execute(update);
  conn.Commit();
}
```