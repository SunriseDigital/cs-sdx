# Sdx.Db.Sql.Delete

## 概要

DELETE文を生成します。

## 使い方

```c#
var db = new Sdx.Db.SqlServerAdapter();
db.ConnectionString = "**接続文字列**";

var delete = db.CreateDelete();

delete
  .SetFrom("shop")
  .Where.Add("id", id);

using (var conn = db.CreateConnection())
{
  conn.Open();
  conn.BeginTransaction();
  var count = conn.Execute(delete);
  conn.Commit();
}
```