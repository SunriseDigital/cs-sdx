# Sdx.Db.Query.Select

## 概要

SELECT文の組み立てを行います。

## 使い方

### テーブルとカラムの指定

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

#### テーブルを指定したカラムの指定

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

### エイリアスの指定

#### テーブル名にエイリアス指定

```c#
var select = db.CreateSelect();

select.From("shop", "s").AddColumn("*");
DbCommand command = select.Build();
```

`DbCommand.CommandText`は下記のようになります。

```sql
SELECT [s].* FROM [shop] AS [s];
```

#### カラムにエイリアス指定

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



