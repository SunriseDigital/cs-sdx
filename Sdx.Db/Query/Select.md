# Sdx.Db.Query.Select

## 概要

SELECT文の組み立てを行います。できるだけ生成できないSQLを減らすように設計されています。

## 使い方

### テーブルとカラム

`Select`は`DbCommand`を生成します。`DbCommand`は`System.Data.Common`名前空間なので参照できるようにします。

```c#
using System.Data.Common;
```

`Select`の生成は`Adapter`から行います。

```c#
var db = new Sdx.Db.SqlServerAdapter();
var select = db.CreateSelect();

select.From("shop");
select.Column("*");
DbCommand command = select.Build();
```

`Select.From()`でテーブル名を指定し、`Select.Column`でカラムを追加します。


`DbCommand.CommandText`は下記のようになります。

```sql
SELECT * FROM [shop];
```

`Select.From()`は複数回呼ぶと追加されていきます。

```c#
select.From("shop");
select.From("category");
select.Column("*");
DbCommand command = select.Build();
```

```sql
SELECT * FROM [shop], [category];
```

#### テーブルを指定したカラムの追加

```c#
var select = db.CreateSelect();

Sdx.Db.Query.Context shopContext = select.From("shop");
shopContext.Column("*");
DbCommand command = select.Build();
```

`Select.From()`は指定したテーブルの`Sdx.Db.Query.Context`オブジェクトを返します。`Context.Column()`でカラムを追加するとテーブルを指定してカラムを追加することが可能です。

生成されるSQLは以下のようになります。

```sql
SELECT [shop].* FROM [shop];
```

複数のカラムを追加する`Columns`メソッドもあります。`Columns`は可変引数を受け付けます。

```c#
var select = db.CreateSelect();

Sdx.Db.Query.Context shopContext =  select.From("shop");

shopContext.Columns("id", "name");
//あるいは
//shopContext.Columns(new string[](){"id", "name"});

DbCommand command = select.Build();
```

`DbCommand.CommandText`は下記のようになります。

```sql
SELECT [shop].[id], [shop].[name] FROM [shop];
```

このように追加したカラムが`*`以外の時は自動でクオートされます。

各カラム系メソッドは自分自身を返しますので、いわゆる[Fluent interface](http://martinfowler.com/bliki/FluentInterface.html)が利用できます。

```c#
var select = db.CreateSelect();

select.From("shop")
  .Column("id")
  .Column("name")
  .Column("category_id");
```

#### クオートを回避する

例えば下記のようなSQLを作りたいとき、自動クオートを回避したいと思います。

```sql
SELECT MAX(shop.id) FROM [shop]
```

その時は`Sdx.Db.Query.Expr`で`string`をラッピングして追加してください。

```c#
select.From("shop");
select.Column(
  Sdx.Db.Query.Expr.Wrap("MAX(shop.id)")
);
```

<br><br><br>
### エイリアス

#### テーブルのエイリアス

```c#
var select = db.CreateSelect();

select.From("shop", "s").Column("*");
DbCommand command = select.Build();
```

`DbCommand.CommandText`は下記のようになります。

```sql
SELECT [s].* FROM [shop] AS [s];
```

#### カラムのエイリアス

カラムにエイリアスを指定する方法は`Select.Column`の第二引数にエイリアス名を渡す方法と`Select.Columns`に`Dictionary<string, object>`を渡す方法と2つあります。

まずは`Select.Column`

```c#
var select = db.CreateSelect();

select.From("shop")
  .Column("id", "shop_id")
  .Column("name", "shop_name")
  ;
DbCommand command = select.Build();
```

`Select.Columns`に`Dictionary<string, object>`を渡す。

```c#
var select = db.CreateSelect();

select.From("shop").Columns(new Dictionary<string, object>(){
  {"shop_id", "id"},
  {"shop_name", "name"},
});

DbCommand command = select.Build();
```

エイリアスが`Dictionary`のキーになりますので気をつけてください。

```sql
SELECT [shop].[id] AS [shop_id], [shop].[name] AS [shop_name] FROM [shop];
```

<br><br><br>
### JOIN

JOINは`Sdx.Db.Query.Context`の`InnerJoin`あるいは`LeftJoin`を使用します。

```c#
var select = db.CreateSelect();

select
  .From("shop")
  .Column("*");

Sdx.Db.Query.Context categoryContext = select.Context("shop")
  .InnderJoin("category", "{0}.category_id = {1}.id")
  .Column("*");
  
DbCommand command = select.Build();
```

```sql
SELECT [shop].* FROM, [category].* [shop] INNER JOIN [category] ON [shop].category_id = [category].id
```

`Select.Context()`は既にJOINしたテーブル（FROM句も含む）の`Context`オブジェクトを取得します。また、`InnerJoin`/`LeftJoin`はJOINしたテーブルの`Context`オブジェクトを返します。

`InnerJoin`/`LeftJoin`の第二引数にはJOINの条件をstringで渡します。string中の`{0}`はクオートされた呼び出し元テーブル（上記の場合`shop`）、`{1}`はクオートされた引数のテーブル（上記の場合`category`）に置換されます。

JOINの条件内のカラム名など、`{0}`/`{1}`を利用したテーブル名以外のテキストはクオートされません。動的な`string`を連結する場合などは、必ず自前でクオーとしてください。

```c#
var db = new Sdx.Db.SqlServerAdapter();
...

select.Context("shop")
  .InnerJoin("category", "{0}."+db.QuoteIdentifier(column)+" = {1}.id");
```

#### 同じテーブルをJOINする

JOINするエイリアス名（テーブル名）は一つの`Select`の中でユニークでなければなりません。同じテーブル名でJOINを２回した場合、上書きされます。

```c#
select.From("shop").Column("*");

select.Context("shop").InnerJoin(
  "category",
  "{0}.category_id = {1}.id"
);

select.Context("shop").InnerJoin(
  "category",
  "{0}.category_id = {1}.id AND {1}.id = 1"
);
db.Command = select.Build();
```

上書きするので`category`のJOINの方が後ろに来ます。

```sql
SELECT
    [shop].*,
    [category].*
FROM
    [shop]
    INNER JOIN
        [image]
    ON  [shop].main_image_id = [image].image_id
    INNER JOIN
        [category]
    ON  [shop].category_id = [category].id
    AND [category].id = 1
```


同じテーブルを複数回JOINする場合はテーブルにエイリアスを付与する必要があります。

```c#
var select = db.CreateSelect();

select
  .From("shop")
  .Column("*");
  
select.Context("shop")
  .InnerJoin("image", "{0}.main_image_id = {1}.id", "main_image")
  .Column("*");

select.Context("shop")
  .InnerJoin("image", "{0}.sub_image_id = {1}.id", "sub_image")
  .Column("*");
```

```sql
SELECT
    [shop].*,
    [main_image].*,
    [sub_image].*
FROM
    [shop]
    INNER JOIN
        [image] AS [main_image]
    ON  [shop].main_image_id = [main_image].id
    INNER JOIN
        [image] AS [sub_image]
    ON  [shop].sub_image_id = [sub_image].id
```

#### JoinOrder

生成されるSQLのJOINの順番は、デフォルトで、`INNER JOIN`を先に、`LEFT JOIN`を後に行います。

```c#
var select = db.CreateSelect();

select
  .From("shop")
  .Column("*");
  
select.Context("shop")
  .LeftJoin("image", "{0}.main_image_id = {1}.id", "main_image");

select.Context("shop")
  .LeftJoin("image", "{0}.sub_image_id = {1}.id", "sub_image");
  
select.Context("shop")
  .InnerJoin("category", "{0}.category_id = {1}.id");
```

```sql
SELECT
    [shop].*
FROM
    [shop]
    INNER JOIN
        [category]
    ON  [shop].category_id = [category].id
    LEFT JOIN
        [image] AS [main_image]
    ON  [shop].main_image_id = [main_image].id
    LEFT JOIN
        [image] AS [sub_image]
    ON  [shop].sub_image_id = [sub_image].id
```

この挙動が気に入らない場合、JOINを呼んだ順番にJOINすることも可能です。

```c#
var select = db.CreateSelect();
select.JoinOrder = Sdx.Db.Query.JoinOrder.Natural;
```

<br><br><br>
### WHERE句

`Select` `Context`共、`Where`というプロパティを持っています。`Where`は`Sdx.Db.Query.Condition`のインスタンスで、一つの`Select`の中では同じインスタンスが参照されます。

`Condition`は`Add`というメソッドを持っていて、これでWhere句をセットしていきます。

```c#
Add(object column, object value, Comparison comparison)
```

| 名前 | 説明 |
| --- | --- |
| column | カラム名。String \| Expr \| Conditionを受け付けます。 |
| value | 値。String \| Int \| IEnumerable<> \| Selectなど、Selectはサブクエリ、IEnumerableはINを生成します。 |
| comparison | 比較演算子。`Sdx.Db.Query.Comparison`enum。省略時は`Sdx.Db.Query.Comparison.Equal` |

```c#
//Comparison
public enum Comparison
{
  Equal,
  NotEqual,
  AltNotEqual,
  GreaterThan,
  LessThan,
  GreaterEqual,
  LessEqual,
  Like,
  NotLike,
  In,
  NotIn
}
```


#### Select.Whereに対する呼び出し

```c#
var select = db.CreateSelect();
select.From("shop").Column("*");
select.Where.Add("id", "1");
```

```sql
SELECT [shop].* FROM [shop] WHERE [id] = @0;
# DbCommand.Parameters["@0"] = 1
```
※プレイスホルダは0から順番に`@数字`がふられます。


#### Context.Whereに対する呼び出し

```c#
var select = db.CreateSelect();
select.From("shop").Column("*");
select.Context("shop").Where.Add("id", "1");
```

```sql
SELECT [shop].* FROM [shop] WHERE [shop].[id] = @0;
# DbCommand.Parameters["@0"] = 1
```

#### IEnumerable<>を使ったINの生成

`Add`の3番目の引数`Comparison`を指定しなくても自動的にINが使用されます。

```c#
var select = db.CreateSelect();
select.From("shop").Column("*");
select.Context("shop").Where.Add("id", new string[] { "1", "2" });
```

```sql
SELECT [shop].* FROM [shop] WHERE [shop].[id] IN (@0, @1);
# DbCommand.Parameters["@0"] = 1
# DbCommand.Parameters["@0"] = 2
```

#### WHERE句にサブクエリ

```c#
var select = db.CreateSelect();
select
  .From("shop")
  .Column("*")
  .Where.Add("id", "1");

var sub = db.CreateSelect();
sub
  .From("category")
  .Column("id")
  .Where.Add("id", "2");

select.Context("shop").Where.Add("category_id", sub, Sdx.Db.Query.Comparison.In);
```

```sql
SELECT
    [shop].*
FROM
    [shop]
WHERE
    [shop].[id] = @0
AND [shop].[category_id] IN(
        SELECT
            [category].[id]
        FROM
            [category]
        WHERE
            [category].[id] = @1
    )

# DbCommand.Parameters["@0"] = 1
# DbCommand.Parameters["@1"] = 2
```

#### ORを含むような複雑なWHERE句

`Where.Add()`に`Sdx.Db.Query.Condition`をセットすると子供のWhere句はカッコで括られます。これを利用するとORを含む複雑なWhere句が生成可能です。`Sdx.Db.Query.Condition`は`Select.CreateCondition()`から生成可能です。

```c#
var select = db.CreateSelect();
select.From("shop").Column("*");

select.Where
  .Add(
    select.CreateCondition()
      .Add("id", "3")
      .Add("id", "4")
  ).AddOr(
    select.CreateCondition()
      .Add("id", "1")
      .AddOr("id", "2")
  );
```

```sql
SELECT
    [shop].*
FROM
    [shop]
WHERE
    ( [id] = @0 AND [id] = @1 )
OR  
    ( [id] = @2 OR  [id] = @3 )
    
# DbCommand.Parameters["@0"] = 3
# DbCommand.Parameters["@1"] = 4
# DbCommand.Parameters["@2"] = 1
# DbCommand.Parameters["@3"] = 2
```

<br><br><br>
### ORDER句

ORDER句は`Select.Order()`、`Context.Order`で行います。`Context`の方はカラムにテーブル名が付与されます。`Order()`は2番めの引数に`Sdx.Db.Query.Order`enumを渡して`ASC`あるいは`DESC`を指定します。

#### Select.Order()

```c#
var select = db.CreateSelect();
select
  .From("shop")
  .Column("*");

select.Order("id", Sdx.Db.Query.Order.DESC);
```

```sql
SELECT [shop].* FROM [shop] ORDER BY [id] DESC
```

#### Context.Order()

```c#
var select = db.CreateSelect();
select
  .From("shop")
  .Column("*")
  .Order("id", Sdx.Db.Query.Order.ASC);
```

```sql
SELECT [shop].* FROM [shop] ORDER BY [shop].[id] ASC
```

<br><br><br>
### GROUP/HAVING句

GROUP句はORDER句同様、`Select.Group()`/`Context.Group()`があります。HAVING句はWHERE句と同様に、`Select.Having`あるいは`Context.Having`プロパティに対して操作を行います。

#### Select.Group()/Select.Having
```c#
select = db.CreateSelect();
select.From("shop");

select
  .Column("id")
  .Group("id")
  .Having.Add(
    Sdx.Db.Query.Expr.Wrap("SUM(shop.id)"),
    10,
    Sdx.Db.Query.Comparison.GreaterEqual
  );
```

```sql
SELECT [id] FROM [shop] GROUP BY [id] HAVING SUM(shop.id) >= @0

# DbCommand.Parameters["@0"] = 10
```

#### Context.Group()/Context.Having
```c#
select = db.CreateSelect();
select
  .From("shop")
  .Column("id")
  .Group("id")
  .Having.Add("id", "2", Sdx.Db.Query.Comparison.GreaterEqual);
```

```sql
SELECT [shop].[id] FROM [shop] GROUP BY [shop].[id] HAVING [shop].[id] >= @0

# DbCommand.Parameters["@0"] = 2
```

<br><br><br>
### LIMIT/OFFSET句

LIMIT/OFFSET句は`Select.Limit`/`Select.Offset`のプロパティにセットします。SqlServerでは、OFFSET/FETCH句が生成されます（SqlServer2012以前では機能しませんので注意してください）。

```c#
var select = db.CreateSelect();
select
  .From("shop")
  .Column("*");

select.Order("id", Sdx.Db.Query.Order.DESC);
select.Limit = 10;
select.Offset = 20;
```

```sql
# SqlServer
SELECT [shop].* FROM [shop] ORDER BY [id] DESC OFFSET 20 ROWS FETCH NEXT 10 ROWS ONLY

# MySql
SELECT `shop`.* FROM `shop` ORDER BY `id` DESC LIMIT 100 OFFSET 10
```

※ SqlServerの仕様でORDER句を付与しないでOFFSET/FETCH句を使うと`System.Data.SqlClient.SqlException`がスローされるので注意してください。

```
System.Data.SqlClient.SqlException: '0' 付近に不適切な構文があります。
FETCH ステートメントのオプション NEXT の使用法が無効です。
```
