# Sdx.Db.Query.Select

## 概要

SELECT文の組み立てを行います。

## 使い方

### テーブルとカラム

`Select`は`DbCommand`を生成します。`DbCommand`は`System.Data.Common`名前空間なので参照できるようにします。

```c#
using System.Data.Common;
```

`Select`の生成は`Factory`から行います。

```c#
var db = new Sdx.Db.SqlServerFactory();
var select = db.CreateSelect();

select.From("shop");
select.AddColumn("*");
DbCommand command = select.Build();
```

`Select.From()`でテーブル名を指定し、`Select.AddColumn`でカラムを追加します。

`DbCommand.CommandText`は下記のようになります。

```sql
SELECT * FROM [shop];
```

#### テーブルを指定したカラムの追加

```c#
var select = db.CreateSelect();

Sdx.Db.Query.Table shopTable = select.From("shop");
shopTable.AddColumn("*");
DbCommand command = select.Build();
```

`Select.From()`は指定したテーブルの`Sdx.Db.Query.Table`オブジェクトを返します。`Table.AddColumn()`でカラムを追加するとテーブルを指定してカラムを追加することが可能です。

生成されるSQLは以下のようになります。

```sql
SELECT [shop].* FROM [shop];
```

複数のカラムを追加する`AddColumns`メソッドもあります。`AddColumns`は可変引数を受け付けます。

```c#
var select = db.CreateSelect();

Sdx.Db.Query.Table shopTable =  select.From("shop");

shopTable.AddColumns("id", "name");
//あるいは
//shopTable.AddColumns(new string[](){"id", "name"});

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
  .AddColumn("id")
  .AddColumn("name")
  .AddColumn("category_id");
```

#### クオートを回避する

例えば下記のようなSQLを作りたいとき、自動クオートを回避したいと思います。

```sql
SELECT MAX(shop.id) FROM [shop]
```

その時は`Sdx.Db.Query.Expr`で`string`をラッピングして追加してください。

```c#
select.From("shop");
select.AddColumn(
  new Sdx.Db.Query.Expr("MAX(shop.id)")
);
```

### エイリアス

#### テーブルのエイリアス

```c#
var select = db.CreateSelect();

select.From("shop", "s").AddColumn("*");
DbCommand command = select.Build();
```

`DbCommand.CommandText`は下記のようになります。

```sql
SELECT [s].* FROM [shop] AS [s];
```

#### カラムのエイリアス

カラムにエイリアスを指定する方法は`Select.AddColumn`の第二引数にエイリアス名を渡す方法と`Select.AddColumns`に`Dictionary<string, object>`を渡す方法と2つあります。

まずは`Select.AddColumn`

```c#
var select = db.CreateSelect();

select.From("shop")
  .AddColumn("id", "shop_id")
  .AddColumn("name", "shop_name")
  ;
DbCommand command = select.Build();
```

`Select.AddColumns`に`Dictionary<string, object>`を渡す。

```c#
var select = db.CreateSelect();

select.From("shop").AddColumns(new Dictionary<string, object>(){
  {"shop_id", "id"},
  {"shop_name", "name"},
});

DbCommand command = select.Build();
```

エイリアスが`Dictionary`のキーになりますので気をつけてください。

```sql
SELECT [shop].[id] AS [shop_id], [shop].[name] AS [shop_name] FROM [shop];
```

### JOIN

JOINは`Sdx.Db.Query.Table`の`InnerJoin`あるいは`LeftJoin`を使用します。

```c#
var select = db.CreateSelect();

select
  .From("shop")
  .AddColumn("*");

Sdx.Db.Query.Table categoryTable = select.Table("shop")
  .InnderJoin("category", "{0}.category_id = {1}.id")
  .AddColumn("*");
  
DbCommand command = select.Build();
```

```sql
SELECT [shop].* FROM, [category].* [shop] INNER JOIN [category] ON [shop].category_id = [category].id
```

`Select.Table()`は既にJOINしたテーブル（FROM句も含む）の`Table`オブジェクトを取得します。また、`InnerJoin`/`LeftJoin`はJOINしたテーブルの`Table`オブジェクトを返します。

`InnerJoin`/`LeftJoin`の第二引数にはJOINの条件をstringで渡します。string中の`{0}`はクオートされた呼び出し元テーブル（上記の場合`shop`）、`{1}`はクオートされた引数のテーブル（上記の場合`category`）に置換されます。

#### 同じテーブルをJOINする

JOINするエイリアス名（テーブル名）は一つの`Select`の中でユニークでなければなりません。同じテーブルを複数回JOINする場合はテーブルにエイリアスを付与する必要があります。

```c#
var select = db.CreateSelect();

select
  .From("shop")
  .AddColumn("*");
  
select.Table("shop")
  .InnerJoin("image", "{0}.main_image_id = {1}.id", "main_image")
  .AddColumn("*");

select.Table("shop")
  .InnerJoin("image", "{0}.sub_image_id = {1}.id", "sub_image")
  .AddColumn("*");
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
    ON  [shop].sub_image_id = [sub_image].id"
```

#### JoinOrder

生成されるSQLのJOINの順番は、デフォルトで、`INNER JOIN`を先に、`LEFT JOIN`を後に行います。

```c#
var select = db.CreateSelect();

select
  .From("shop")
  .AddColumn("*");
  
select.Table("shop")
  .LeftJoin("image", "{0}.main_image_id = {1}.id", "main_image");

select.Table("shop")
  .LeftJoin("image", "{0}.sub_image_id = {1}.id", "sub_image");
  
select.Table("shop")
  .InnerJoin("category", );
```
