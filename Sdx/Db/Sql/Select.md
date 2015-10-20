# Sdx.Db.Query.Select

## 概要

SELECT文を生成します。

## 使い方

### テーブルとカラム

`Select`は`DbCommand`を生成します。`DbCommand`は`System.Data.Common`名前空間なので参照できるようにします。

```c#
using System.Data.Common;
```

`Select`は最終的に`Build`メソッドでSQLを生成しますが、DB毎に識別子のクオートの仕方が違うため`Adapter`のインスタンスが必要です。

```c#
var select = new Sdx.Db.Query.Select();

select.AddFrom("shop");
select.AddColumn("*");
select.Adapter = new Sdx.Db.SqlServerAdapter();
select.Adapter.ConnectionString = "**接続文字列**";
DbCommand command = select.Build();
```
※以下、全ての例で`ConnectionString`の設定は省略します。

`Select.AddFrom()`でテーブル名を指定し、`Select.AddColumn`でカラムを追加します。


`DbCommand.CommandText`は下記のようになります。

```sql
SELECT * FROM [shop];
```

`Select.AddFrom()`は複数回呼ぶと追加されていきます。

```c#
select.AddFrom("shop");
select.AddFrom("area");
select.AddColumn("*");
DbCommand command = select.Build();
```

```sql
SELECT * FROM [shop], [area];
```

#### テーブルを指定したカラムの追加



```c#
//Adapterを引数に取るコンストラクタも有ります。
var select = new Sdx.Db.Query.Select(new Sdx.Db.SqlServerAdapter());

Sdx.Db.Query.Context shopContext = select.AddFrom("shop");
shopContext.AddColumn("*");
DbCommand command = select.Build();
```

`Select.AddFrom()`は指定したテーブルの`Sdx.Db.Query.Context`オブジェクトを返します。`Context.AddColumn()`でカラムを追加するとテーブルを指定してカラムを追加することが可能です。

生成されるSQLは以下のようになります。

```sql
SELECT [shop].* FROM [shop];
```

複数のカラムを追加する`AddColumns`メソッドもあります。`AddColumns`は可変引数を受け付けます。

```c#
var select = new Sdx.Db.Query.Select(new Sdx.Db.SqlServerAdapter());

Sdx.Db.Query.Context shopContext =  select.AddFrom("shop");

shopContext.AddColumns("id", "name");
//あるいは
//shopContext.AddColumns(new string[](){"id", "name"});

DbCommand command = select.Build();
```

`DbCommand.CommandText`は下記のようになります。

```sql
SELECT [shop].[id], [shop].[name] FROM [shop];
```

このように追加したカラムが`*`以外の時は自動でクオートされます。

各カラム系メソッドは自分自身を返しますので、いわゆる[Fluent interface](http://martinfowler.com/bliki/FluentInterface.html)が利用できます。

```c#
var select = new Sdx.Db.Query.Select(new Sdx.Db.SqlServerAdapter());

select.AddFrom("shop")
  .AddColumn("id")
  .AddColumn("name")
  .AddColumn("area_id");
```

#### クオートを回避する

例えば下記のようなSQLを作りたいとき、自動クオートを回避したいと思います。

```sql
SELECT MAX(shop.id) FROM [shop]
```

その時は`Sdx.Db.Query.Expr`で`string`をラッピングして追加してください。

```c#
select.AddFrom("shop");
select.AddColumn(
  Sdx.Db.Query.Expr.Wrap("MAX(shop.id)")
);
```

<br><br><br>
### エイリアス

#### テーブルのエイリアス

```c#
var select = new Sdx.Db.Query.Select(new Sdx.Db.SqlServerAdapter());

select.AddFrom("shop", "s").AddColumn("*");
DbCommand command = select.Build();
```

`DbCommand.CommandText`は下記のようになります。

```sql
SELECT [s].* FROM [shop] AS [s];
```

#### カラムのエイリアス

カラムにエイリアスを指定する方法は`Select.AddColumn`の第二引数にエイリアス名を渡します。`Select.AddColumns`ではエイリアスの付与ができません。

まずは`Select.AddColumn`

```c#
var select = new Sdx.Db.Query.Select(new Sdx.Db.SqlServerAdapter());

select.AddFrom("shop")
  .AddColumn("id", "shop_id")
  .AddColumn("name", "shop_name")
  ;
DbCommand command = select.Build();
```

```sql
SELECT [shop].[id] AS [shop_id], [shop].[name] AS [shop_name] FROM [shop];
```

<br><br><br>
### JOIN

JOINは`Sdx.Db.Query.Context`の`InnerJoin`あるいは`LeftJoin`を使用します。

```c#
var select = new Sdx.Db.Query.Select(new Sdx.Db.SqlServerAdapter());

select
  .AddFrom("shop")
  .AddColumn("*");

Sdx.Db.Query.Context areaContext = select.Context("shop")
  .InnerJoin(
    "area",
    db.CreateCondition().Add(
      new Sdx.Db.Query.Column("area_id", "shop"),
      new Sdx.Db.Query.Column("id", "area")
    )
    .Add(
      new Sdx.Db.Query.Column("id", "area"),
      "1"
    )
  )
  .AddColumn("*");

DbCommand command = select.Build();
```

```sql
SELECT [shop].* FROM, [area].* [shop] INNER JOIN [area] ON [shop].area_id = [area].id AND [area].[id] = @0
# DbCommand.Parameters["@0"] = "1";
```

`InnerJoin`/`LeftJoin`はJOINしたテーブルの`Context`オブジェクトを返します。`Select.Context()`はFROM句、あるいはJOIN句のテーブルの`Context`オブジェクトを取得するメソッドです。

`InnerJoin`/`LeftJoin`の第二引数にはJOINの条件を`Sdx.Db.Query.Condition`のインスタンスで渡します。`Condition`は`*** = @@@`の様な条件式を生成する汎用的なクラスです。`Condition`にはJOIN条件の条件の他にWhere句やHaving句の生成にも利用されます。JOIN条件でよく使用する`column_name1 = column_name2`の式を生成するには、`Sdx.Db.Query.Column`のインスタンスを両方の引数に設定してください。

※ `Condition`の生成には本来`Adapter`は必要ではありません。`Adapter.CreateCondition()`は`(new Sdx.Db.Query.Condition()).Add(...`と記述しなくてもいいようにするためのショートカットです。識別子のクオートには`Select.Adapter`が使用されます。


#### 同じテーブルをJOINする

JOINするエイリアス名（テーブル名）は一つの`Select`の中でユニークでなければなりません。同じテーブル名でJOINを２回した場合、一度しかJOINは行われません。

```c#
select.AddFrom("shop").AddColumn("*");

//まずはareaをJOIN
select.Context("shop").InnerJoin(
  "area",
  db.CreateCondition().Add(
    new Sdx.Db.Query.Column("area_id", "shop"),
    new Sdx.Db.Query.Column("id", "area")
  )
);

//次にはimageをJOIN
select.Context("shop").InnerJoin(
  "image",
  db.CreateCondition().Add(
    new Sdx.Db.Query.Column("main_image_id", "shop"),
    new Sdx.Db.Query.Column("id", "image")
  )
);

//もう一度areaをJOIN
select.Context("shop").InnerJoin(
  "area",
  db.CreateCondition().Add(
    new Sdx.Db.Query.Column("area_id", "shop"),
    new Sdx.Db.Query.Column("id", "area")
  )
  .Add(
    new Sdx.Db.Query.Column("id", "area"),
    1
  )
);
db.Command = select.Build();
```

同じテーブルを同じ名前でJOINすると、上書きするので`area`のJOINの方が後ろに来ます。

```sql
SELECT
    [shop].*,
    [area].*
FROM
    [shop]
    INNER JOIN
        [image]
    ON  [shop].main_image_id = [image].image_id
    INNER JOIN
        [area]
    ON  [shop].area_id = [area].id
    AND [area].[id] = @0

# DbCommand.Paramters["@0"] = "1"
```


同じテーブルを複数回JOINする場合はテーブルにエイリアスを付与する必要があります。

```c#
var select = new Sdx.Db.Query.Select(new Sdx.Db.SqlServerAdapter());

select
  .AddFrom("shop")
  .AddColumn("*");

select.Context("shop")
  .InnerJoin("image", db.CreateCondition().Add(
    new Sdx.Db.Query.Column("main_image_id", "shop"),
    new Sdx.Db.Query.Column("id", "image")
  ), "main_image")
  .AddColumn("*");

select.Context("shop")
  .InnerJoin("image", db.CreateCondition().Add(
    new Sdx.Db.Query.Column("sub_image_id", "shop"),
    new Sdx.Db.Query.Column("id", "image")
  ), "sub_image")
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
var select = new Sdx.Db.Query.Select(new Sdx.Db.SqlServerAdapter());

select
  .AddFrom("shop")
  .AddColumn("*");

select.Context("shop")
  .LeftJoin("image", db.CreateCondition().Add(
    new Sdx.Db.Query.Column("main_image_id", "shop"),
    new Sdx.Db.Query.Column("id", "image")
  ), "main_image");

select.Context("shop")
  .LeftJoin("image", db.CreateCondition().Add(
    new Sdx.Db.Query.Column("sub_image_id", "shop"),
    new Sdx.Db.Query.Column("id", "image")
  ), "sub_image");

select.Context("shop")
  .InnerJoin("area", db.CreateCondition().Add(
    new Sdx.Db.Query.Column("area_id", "shop"),
    new Sdx.Db.Query.Column("id", "area")
  ));
```

```sql
SELECT
    [shop].*
FROM
    [shop]
    INNER JOIN
        [area]
    ON  [shop].area_id = [area].id
    LEFT JOIN
        [image] AS [main_image]
    ON  [shop].main_image_id = [main_image].id
    LEFT JOIN
        [image] AS [sub_image]
    ON  [shop].sub_image_id = [sub_image].id
```

この挙動が気に入らない場合、JOINを呼んだ順番にJOINすることも可能です。

```c#
var select = new Sdx.Db.Query.Select(new Sdx.Db.SqlServerAdapter());
select.JoinOrder = Sdx.Db.Query.JoinOrder.Natural;
```

#### JOINのfluent syntaxについて

InnerJoin/LeftJoinの返り値は、メソッドの呼び出しでJOINしたテーブルの`Context`なので、注意して下さい。

```c#
select.AddFrom("shop")
  .InnerJoin("area", db.CreateCondition().Add(
    new Sdx.Db.Query.Column("area_id", "shop"),
    new Sdx.Db.Query.Column("id", "area")
  ))
  .AddColumn("id", 1)//これはarea.id
```

上記の例の`.AddColumn("id")`は`shop`ではなく、`area`の`id`です。`area`の`Context`から`shop`の`Context`にアクセスするには`ParentContext`プロパティを使うか`Select`プロパティを利用します。

```c#
select.AddFrom("shop")
  .InnerJoin("area", db.CreateCondition().Add(
    new Sdx.Db.Query.Column("area_id", "shop"),
    new Sdx.Db.Query.Column("id", "area")
  )).ParentContext
  .AddColumn("id", 1)//これはshop.id
```

```c#
select.AddFrom("shop")
  .InnerJoin("area", db.CreateCondition().Add(
    new Sdx.Db.Query.Column("area_id", "shop"),
    new Sdx.Db.Query.Column("id", "area")
  )).Select.Context("shop")
  .AddColumn("id", 1)//これはshop.id
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
var select = new Sdx.Db.Query.Select(new Sdx.Db.SqlServerAdapter());
select.AddFrom("shop").AddColumn("*");
select.Where.Add("id", "1");
```

```sql
SELECT [shop].* FROM [shop] WHERE [id] = @0;
# DbCommand.Parameters["@0"] = 1
```
※プレイスホルダは0から順番に`@数字`がふられます。


#### Context.Whereに対する呼び出し

```c#
var select = new Sdx.Db.Query.Select(new Sdx.Db.SqlServerAdapter());
select.AddFrom("shop").AddColumn("*");
select.Context("shop").Where.Add("id", "1");
```

```sql
SELECT [shop].* FROM [shop] WHERE [shop].[id] = @0;
# DbCommand.Parameters["@0"] = 1
```

#### IEnumerable<>を使ったINの生成

`Add`の3番目の引数`Comparison`を指定しなくても自動的にINが使用されます。

```c#
var select = new Sdx.Db.Query.Select(new Sdx.Db.SqlServerAdapter());
select.AddFrom("shop").AddColumn("*");
select.Context("shop").Where.Add("id", new string[] { "1", "2" });
```

```sql
SELECT [shop].* FROM [shop] WHERE [shop].[id] IN (@0, @1);
# DbCommand.Parameters["@0"] = 1
# DbCommand.Parameters["@0"] = 2
```

#### WHERE句にサブクエリ

```c#
var select = new Sdx.Db.Query.Select(new Sdx.Db.SqlServerAdapter());
select
  .AddFrom("shop")
  .AddColumn("*")
  .Where.Add("id", "1");

var sub = new Sdx.Db.Query.Select(new Sdx.Db.SqlServerAdapter());
sub
  .AddFrom("area")
  .AddColumn("id")
  .Where.Add("id", "2");

select.Context("shop").Where.Add("area_id", sub, Sdx.Db.Query.Comparison.In);
```

```sql
SELECT
    [shop].*
FROM
    [shop]
WHERE
    [shop].[id] = @0
AND [shop].[area_id] IN(
        SELECT
            [area].[id]
        FROM
            [area]
        WHERE
            [area].[id] = @1
    )

# DbCommand.Parameters["@0"] = 1
# DbCommand.Parameters["@1"] = 2
```

#### ORを含むような複雑なWHERE句

`Where.Add()`に`Sdx.Db.Query.Condition`をセットすると子供のWhere句はカッコで括られます。これを利用するとORを含む複雑なWhere句が生成可能です。`Sdx.Db.Query.Condition`は`Adapter.CreateCondition()`から生成可能です。

```c#
var select = new Sdx.Db.Query.Select(new Sdx.Db.SqlServerAdapter());
select.AddFrom("shop").AddColumn("*");

select.Where
  .Add(
    select.Adapter.CreateCondition()
      .Add("id", "3")
      .Add("id", "4")
  ).AddOr(
    select.Adapter.CreateCondition()
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

#### Whereのfluent syntaxについて

Where.Add系メソッドは続けて条件を付与できるようにするため、自分自身を返します。`Context`にアクセスする時は`Context`プロパティを利用してください。

```c#
select.AddFrom("shop")
  .Where.Add("id", 1).Context
  .InnerJoin("area", db.CreateCondition().Add(
    new Sdx.Db.Query.Column("area_id", "shop"),
    new Sdx.Db.Query.Column("id", "area")
  ));
```

<br><br><br>
### ORDER句

ORDER句は`Select.AddOrder()`、`Context.AddOrder`で行います。`Context`の方はカラムにテーブル名が付与されます。`Order()`は2番めの引数に`Sdx.Db.Query.AddOrder`enumを渡して`ASC`あるいは`DESC`を指定します。

#### Select.AddOrder()

```c#
var select = new Sdx.Db.Query.Select(new Sdx.Db.SqlServerAdapter());
select
  .AddFrom("shop")
  .AddColumn("*");

select.AddOrder("id", Sdx.Db.Query.AddOrder.DESC);
```

```sql
SELECT [shop].* FROM [shop] ORDER BY [id] DESC
```

#### Context.AddOrder()

```c#
var select = new Sdx.Db.Query.Select(new Sdx.Db.SqlServerAdapter());
select
  .AddFrom("shop")
  .AddColumn("*")
  .AddOrder("id", Sdx.Db.Query.AddOrder.ASC);
```

```sql
SELECT [shop].* FROM [shop] ORDER BY [shop].[id] ASC
```

<br><br><br>
### GROUP/HAVING句

GROUP句はORDER句同様、`Select.AddGroup()`/`Context.AddGroup()`があります。HAVING句はWHERE句と同様に、`Select.Having`あるいは`Context.Having`プロパティに対して操作を行います。

#### Select.AddGroup()/Select.Having
```c#
select = new Sdx.Db.Query.Select(new Sdx.Db.SqlServerAdapter());
select.AddFrom("shop");

select
  .AddColumn("id")
  .AddGroup("id")
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

#### Context.AddGroup()/Context.Having
```c#
select = new Sdx.Db.Query.Select(new Sdx.Db.SqlServerAdapter());
select
  .AddFrom("shop")
  .AddColumn("id")
  .AddGroup("id")
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
var select = new Sdx.Db.Query.Select(new Sdx.Db.SqlServerAdapter());
select
  .AddFrom("shop")
  .AddColumn("*");

select.AddOrder("id", Sdx.Db.Query.AddOrder.DESC);
select.SetLimit(10, 20);
```

最初の引数がLIMIT句、2番めの引数がOFFSET句です。OFFSETは省略可能です。

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
