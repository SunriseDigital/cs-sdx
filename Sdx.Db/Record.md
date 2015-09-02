# Sdx.Db.Record

## 概要

各テーブルの一行を表現したクラス。`RecordSet`は複数の`Record`のコレクションです。

## 使い方

### クラスの準備

`Record`は`Adapter.FetchRecord<T:Record>(DbCommand command)`あるいは`Adapter.FetchRecord<T:Record>(Select select)`で生成しますが、組み立てるのにテーブルの定義が必要です。テーブルの定義は`Sdx.Db.Table`クラスのサブクラスを作成して行います。

`Record`クラス自体も、各テーブルごとに固有のユーティリティーメソッドを持たせたいので、サブクラスを作成します。

下記のDBを元に解説をします。

 ![ER図](er.png "ER図")

#### テーブルクラス

基本的な設定です。テーブルクラス自体は各SELECTの中でJOINに利用され、JOIN時のデータを保有するため、複数生成されるクラスです。テーブルの定義自体はユニークなものなので`Sdx.Db.MetaData`クラスのインスタンスとしてstaticなプロパティに保持します。本来はテーブル同士の関係も定義しますが、話を単純にするため、最初の例では省略します。

```c#
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Shop : Sdx.Db.Table
  {
    public static Sdx.Db.MetaData Meta { get; private set; }

    static Shop()
    {
      Meta =  new Sdx.Db.MetaData(
        "shop",
        new List<string>()
        {
          "id"
        },
        new List<Column>()
        {
          new Column("id"),
          new Column("name"),
          new Column("area_id"),
          new Column("main_image_id"),
          new Column("sub_image_id"),
        },
        new Dictionary<string, Relation>()
        {
          //他テーブルとの関係を設定します。
        },
        typeof(Test.Orm.Shop),
        typeof(Test.Orm.Table.Category)
      );
    }
  }
}
```

`MetaData`コンストラクタの引数は下記のようになっています。

1. テーブル名
1. 主キー
1. カラムのリスト
1. 他テーブルとの関連（`Relation`）
1. レコードクラスのタイプ
1. テーブルクラスのタイプ

#### レコードクラス

レコードクラスからもテーブル定義にアクセスできるようするためstaticなプロパティが必要です。

```c#
namespace Test.Orm
{
  public class Shop : Sdx.Db.Record
  {
    public static Sdx.Db.MetaData Meta { get; private set; }

    static Shop()
    {
      Meta = Test.Orm.Table.Shop.Meta;
    }
  }
}
```

#### リレーションを貼る

`shop`は複数の`menu`を持っています。この関係を設定してみます。

```c#
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Shop : Sdx.Db.Table
  {
    public static Sdx.Db.MetaData Meta { get; private set; }

    static Shop()
    {
      Meta =  new Sdx.Db.MetaData(
        "shop",
        new List<string>()
        {
          "id"
        },
        new List<Column>()
        {
          new Column("id"),
          new Column("name"),
          new Column("area_id"),
          new Column("main_image_id"),
          new Column("sub_image_id"),
        },
        new Dictionary<string, Relation>()
        {
          {
            "menu",
            new Relation(
              typeof(Test.Orm.Table.Menu),
              "id",
              "shop_id"
            )
          }
        },
        typeof(Test.Orm.Shop),
        typeof(Test.Orm.Table.Category)
      );
    }
  }
}
```

`Sdx.Db.Table.Relation`のインスタンスで関連を設定します。コンストラクタの引数は下記のようになっています。

1. テーブルクラスのType
1. 自分自身の参照カラム。`shop.id`になります。
1. 相手テーブルの参照ラカム。`menu.shop_id`です。

このようにして、テーブル同士の関係を設定していきます。[ユニットテストのテーブルクラス](../UnitTest/Test/Orm/Table)で様々な設定を確認できますので参考にしてください。



<br><br><br>
### SELECTの組み立て


