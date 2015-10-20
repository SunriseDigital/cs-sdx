# Sdx.Db.Sql.Insert

## 概要

Insert文を生成します。

## 使い方

```c#
var db = new Sdx.Db.SqlServerAdapter();
db.ConnectionString = "**接続文字列**";

var insert = db.CreateInsert();

insert
   .SetInto("shop")
   .AddColumnValue("name", "NewShop")
   .AddColumnValue("area_id", 1)
   .AddColumnValue("created_at", DateTime.Now);

using (var conn = db.CreateConnection())
{
  conn.Open();
  conn.BeginTransaction();
  var count = conn.Execute(insert);
  conn.Commit();
}
```