# Sdx.Db.Query.Select

## 概要

SELECT文の組み立てを行います。できるだけ生成できないSQLを減らすように設計されています。

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

`Select.From()`は複数回呼ぶと追加されていきます。

```c#
select.From("shop");
select.From("category");
select.AddColumn("*");
DbCommand command = select.Build();
```

```sql
SELECT * FROM [shop], [category];
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
  Sdx.Db.Query.Expr.Wrap("MAX(shop.id)")
);
```

<br><br><br>
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

<br><br><br>
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

JOINの条件内のカラム名など、`{0}`/`{1}`を利用したテーブル名以外のテキストはクオートされません。動的な`string`を連結する場合などは、必ず自前でクオーとしてください。

```c#
var db = new Sdx.Db.SqlServerFactory();
...

select.Table("shop")
  .InnerJoin("category", "{0}."+db.QuoteIdentifier(column)+" = {1}.id");
```

#### 同じテーブルをJOINする

JOINするエイリアス名（テーブル名）は一つの`Select`の中でユニークでなければなりません。同じテーブル名でJOINを２回した場合、上書きされます。

```c#
select.From("shop").AddColumn("*");

select.Table("shop").InnerJoin(
  "category",
  "{0}.category_id = {1}.id"
);

select.Table("shop").InnerJoin(
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
    ON  [shop].sub_image_id = [sub_image].id
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

`Select` `Table`共、`Where`というプロパティを持っています。`Where`は`Sdx.Db.Query.Where`のインスタンスで、一つの`Select`の中では同じインスタンスが参照されます。

`Where`は`Add`というメソッドを持っていて、これでWhere句をセットしていきます。

```c#
Add(object column, object value, Comparison comparison)
```

| 名前 | 説明 |
| --- | --- |
| column | カラム名。String \| Expr \| Whereを受け付けます。 |
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
select.From("shop").AddColumn("*");
select.Where.Add("id", "1");
```

```sql
SELECT [shop].* FROM [shop] WHERE [id] = @0;
# DbCommand.Parameters["@0"] = 1
```
※プレイスホルダは0から順番に`@数字`がふられます。


#### Table.Whereに対する呼び出し

```c#
var select = db.CreateSelect();
select.From("shop").AddColumn("*");
select.Table("shop").Where.Add("id", "1");
```

```sql
SELECT [shop].* FROM [shop] WHERE [shop].[id] = @0;
# DbCommand.Parameters["@0"] = 1
```

#### IEnumerable<>を使ったINの生成

`Add`の3番目の引数`Comparison`を指定しなくても自動的にINが使用されます。

```c#
var select = db.CreateSelect();
select.From("shop").AddColumn("*");
select.Table("shop").Where.Add("id", new string[] { "1", "2" });
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
  .AddColumn("*")
  .Where.Add("id", "1");

var sub = db.CreateSelect();
sub
  .From("category")
  .AddColumn("id")
  .Where.Add("id", "2");

select.Table("shop").Where.Add("category_id", sub, Sdx.Db.Query.Comparison.In);
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

`Where.Add()`に`Where`をセットすると子供の`Where`はカッコで括られます。これを利用するとORを含む複雑なWhere句が生成可能です。`Where`は`Select.CreateWhere()`から生成可能です。

```c#
var select = db.CreateSelect();
select.From("shop").AddColumn("*");

select.Where
  .Add(
    select.CreateWhere()
      .Add("id", "3")
      .Add("id", "4")
  ).AddOr(
    select.CreateWhere()
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

ORDER句は`Select.Order()`、`Table.Order`で行います。`Table`の方はカラムにテーブル名が付与されます。`Order()`は2番めの引数に`Sdx.Db.Query.Order`enumを渡して`ASC`あるいは`DESC`を指定します。

#### Select.Order()

```c#
var select = db.CreateSelect();
select
  .From("shop")
  .AddColumn("*");

select.Order("id", Sdx.Db.Query.Order.DESC);
```

```sql
SELECT [shop].* FROM [shop] ORDER BY [id] DESC
```

#### Table.Order()

```c#
var select = db.CreateSelect();
select
  .From("shop")
  .AddColumn("*")
  .Order("id", Sdx.Db.Query.Order.ASC);
```

```sql
SELECT [shop].* FROM [shop] ORDER BY [shop].[id] ASC
```

<br><br><br>
### GROUP句

ORDER句同様、`Select.Order()`/`Tabl.Order()`があります。

#### Select.Group()
```c#
select = db.Factory.CreateSelect();
select.From("shop");

select
  .AddColumn("id")
  .Group("id");
```

```sql
SELECT [id] FROM [shop] GROUP BY [id]
```

#### Table.Group()
```c#
select = db.Factory.CreateSelect();
select
  .From("shop")
  .AddColumn("id")
  .Group("id");
```

```sql
SELECT [shop].[id] FROM [shop] GROUP BY [shop].[id]
```

