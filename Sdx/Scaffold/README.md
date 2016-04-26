# Sdx.Scaffold

## 概要

Scaffoldとは建設現場の「足場」を意味します。テーブル単位でデータの登録・編集の管理画面を簡単に生成するシステムですが、 余り複雑な画面は生成出来ないため、システムのメインデータでは使いません。 メインデータの登録画面を組み上げるのに必要なマスターデータ等の管理画面（足場）を生成するのに使われるためScaffoldと呼ばれています。

説明は[こちら](../Db/Record.md)のテーブル定義を元に行います。

## 使い方

### セットアップ

ページのHTML部分にUserControlを使用していたりJavascriptやCSSなども必要なため、DLLには組み込まず、独立したリポジトリ[sdxweb](https://github.com/SunriseDigital/sdxweb)になっています。これをWebから参照できるフォルダに展開してください。カスタマイズは可能ですが、デフォルトではWebドキュメントルートの`sdx`というディレクトリに展開されることを想定しています。

```
docroot/
  sdx/
    _private/
    css/
    js/
  Default.aspx
  Web.Config
```

`_private`の中はウェブからアクセスされることを想定していないソースなどが置かれています。ウェブサーバーがApacheの場合既にhtaccessでアクセスが拒否されていますが、他のサーバーの場合は別途拒否するよう設定をしてください。

IISではgitにコミットできるような形でアクセスを拒否する方法が見つかりませんでした（ありましたらPRいただけると嬉しいです）。IISで`_private`を見れなくするには下記のようにします。

1. `_private`ディレクトリを右クリックしプロパティを表示する。
1. セキュリティータブに切り替え、編集をクリック。
1. Administarators、SYSTEM 及び個別のユーザー・グループだけに許可をして、`Users`、`Authenticated Users`、`IUSR`、`IIS_IUSRS`等のアクセス権については削除する。
1. 追加を押し、「選択するオブジェクト名を入力してください」のところに`IIS AppPool\使用したいアプリのアプリケーションプール名`を入力しOKを押す。これをしないとUserControlのインクルードに失敗しますので注意してください。

<br><br><br>
### ページを準備する

データのリストを表示するページと、編集ページ、２ページで構成されます。サンプルでは`Test.Orm.Shop`の管理画面を生成します。`/scaffold/shop/list.aspx` `/scaffold/shop/edit.aspx`を作成してください。

```
docroot/
  scaffold/
    shop/
      list.aspx
      edit.aspx
  sdx/
    _private/
    css/
    js/
  Default.aspx
  Web.Config
```

#### リストページ

`/scaffold/shop/list.aspx`
```asp.net
<%@ Page Language="C#" AutoEventWireup="true" CodeFile="list.aspx.cs" Inherits="scaffold_shop_list" %>
<%@ Register TagPrefix="Scaffold" TagName="list" Src="~\sdx\_private\csharp\scaffold\list.ascx" %>
<%@ Register TagPrefix="Scaffold" TagName="head" Src="~\sdx\_private\csharp\scaffold\head.ascx" %>
<!DOCTYPE html>
<html>
  <head>
    <meta charset="utf-8">
    <title></title>
    <Scaffold:head ID="head" runat="server" />
  </head>
  <body>
    <div class="container">
      <Scaffold:list ID="list" runat="server" OutlineRank=3 />
    </div>
  </body>
</html>
```

sdxのscaffoldではページのデザインに[Bootstrap](http://getbootstrap.com/)を使用しています。他、必要なCSSやJavascriptを読み込むため、`head.ascx`をページに読み込む必要があります。実際のリストのHTML部分は`list.ascx`です。任意の場所に読み込んでください。`OutlineRank`でリスト部分の最上位のアウトラインランクを指定可能です（省略すると1）。サンプルでは`3`となっていますので`h3`タグから付与されていきます。

`/scaffold/shop/list.aspx.cs`
```c#
public partial class scaffold_shop_list : System.Web.UI.Page
{
  protected void Page_Load(object sender, EventArgs e)
  {
    Test.Scaffold.Shop.Create();
  }
}
```

このサンプルでは`Test.Scaffold.Shop`という生成用のScaffoldファクトリクラスを作りました。`Create()`は後ほど示しますが、`Sdx.Scaffold.Manager`を組み立ててるだけです。

`Sdx.Scaffold.Manager`のインスタンスは生成するだけで`HttpContext`に保持されUserControlに関連付けられますので、とくにメンバー変数に代入する必要はありません。


#### 編集ページ

`/scaffold/shop/edit.aspx`
```asp.net
<%@ Page Language="C#" AutoEventWireup="true" CodeFile="edit.aspx.cs" Inherits="scaffold_shop_edit" %>
<%@ Register TagPrefix="Scaffold" TagName="edit" Src="~\sdx\_private\csharp\scaffold\edit.ascx" %>
<%@ Register TagPrefix="Scaffold" TagName="head" Src="~\sdx\_private\csharp\scaffold\head.ascx" %>
<!DOCTYPE html>

<html>
  <head>
    <meta charset="utf-8">
    <title></title>
    <Scaffold:head ID="head" runat="server" />
  </head>
  <body>
    <div class="container">
      <Scaffold:edit ID="edit" runat="server" OutlineRank=3 />
    </div>
  </body>
</html>
```

`/scaffold/shop/edit.aspx.cs`
```c#
public partial class scaffold_shop_edit : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
      Test.Scaffold.Shop.Create();
    }
}
```

#### Scaffoldファクトリ

`Sdx.Scaffold.Manager`に設定情報を付与していきます。`Sdx.Scaffold.Manager`の初期化には、対象のテーブルの`Sdx.Db.TableMeta`と保存先の`Sdx.Db.Adapter`のインスタンスが必要です。

リストはレスポンシブを想定しているため[resplist](https://github.com/SunriseDigital/sdxweb/blob/master/_private/sass/resplist.scss)を使用しています。幅を指定しないと縦の列がきれいに揃いませんので注意してください。

`Test.Scaffold.Shop`
```c#
namespace Test.Scaffold
{
  public class Shop
  {
    public static Sdx.Scaffold.Manager Create()
    {
      var db = new Sdx.Db.Adapter.SqlServer();
      db.ConnectionString = "**接続文字列**";

      var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Shop.Meta, db);

      //見出し文字列
      scaffold.Title = "店舗";

      //ページの行き来をするのにお互いのURLが必要です。
      scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/shop/edit.aspx");
      scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/shop/list.aspx");

      //リストに表示する項目の設定
      scaffold.DisplayList
        //各表示項目はSdx.Scaffold.Config.Itemのインスタンスで追加します。
        .Add(Sdx.Scaffold.Config.Item.Create()
          //一つの項目に幾つかの設定値があり、設定名と合わせてSdx.Scaffold.Config.Valueのインスタンスでセットします。
          //`label`は見出しです。
          .Set("label", new Sdx.Scaffold.Config.Value("ID"))
          //`column`はRecord.GetString(columnName)が呼ばれます。
          .Set("column", new Sdx.Scaffold.Config.Value("id"))
          //`style`は項目のstyle属性にそのまま付与されます。
          .Set("style", new Sdx.Scaffold.Config.Value("width: 70px;"))
        )
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Sdx.Scaffold.Config.Value("名前"))
          .Set("column", new Sdx.Scaffold.Config.Value("name"))
          .Set("style", new Sdx.Scaffold.Config.Value("width: 120px;"))
        )
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Sdx.Scaffold.Config.Value("エリア"))
          //`dynamic`はReocrd.GetDynamic()が呼ばれます。
          //`@area.name` = `shop.GetRecord('area').GetString("name")`
          .Set("dynamic", new Sdx.Scaffold.Config.Value("@area.name"))
          .Set("style", new Sdx.Scaffold.Config.Value("width: 80px;"))
        )
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Sdx.Scaffold.Config.Value("業種"))
          //`#GetCategoryNames` = `shop.GetCategoryNames(conn)`
          //Shopのレコードに`public string GetCategoryNames(Sdx.Db.Connection conn = null)`メソッドが実装されています。
          .Set("dynamic", new Sdx.Scaffold.Config.Value("#GetCategoryNames"))
          .Set("style", new Sdx.Scaffold.Config.Value("width: 150px;"))
          //`class`は追加でclass属性を付与します。柔軟にスタイルを利かせたい場合に使用します。
          .Set("class", new Sdx.Scaffold.Config.Value("category"))
        )
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Sdx.Scaffold.Config.Value("ログインID"))
          .Set("column", new Sdx.Scaffold.Config.Value("login_id"))
          .Set("style", new Sdx.Scaffold.Config.Value("width: 100px;"))
        )
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Sdx.Scaffold.Config.Value("登録日時"))
          .Set("column", new Sdx.Scaffold.Config.Value("created_at"))
          .Set("style", new Sdx.Scaffold.Config.Value("width: 170px;"))
        );

      //一ページ１０件のページネーションが付与される。省略すると全件表示されます。
      scaffold.PerPage = 10;

      //リストの並び順は`ListSelectHook`で指定。
      scaffold.ListSelectHook = new Sdx.Scaffold.Config.Value((select, conn) =>
      {
        select.Context("shop").AddOrder("id", Sdx.Db.Sql.Order.ASC);
      });

      //下記のように文字列で指定すると`Table.SelectDefaultOrder(Select select, Connection conn = null)`が呼ばれます。
      //scaffold.ListSelectHook = new Sdx.Scaffold.Config.Value("SelectDefaultOrder");

      //ここから保存ページの設定。
      //保存用のフォーム項目設定
      scaffold.FormList
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("column", new Sdx.Scaffold.Config.Value("name"))
          .Set("label", new Sdx.Scaffold.Config.Value("店名"))
        ).Add(Sdx.Scaffold.Config.Item.Create()
          .Set("column", new Sdx.Scaffold.Config.Value("area_id"))
          .Set("label", new Sdx.Scaffold.Config.Value("場所"))
        ).Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Sdx.Scaffold.Config.Value("業種"))
          .Set("relation", new Sdx.Scaffold.Config.Value("shop_category"))
          .Set("column", new Sdx.Scaffold.Config.Value("category_id"))
        ).Add(Sdx.Scaffold.Config.Item.Create()
          .Set("column", new Sdx.Scaffold.Config.Value("login_id"))
          .Set("label", new Sdx.Scaffold.Config.Value("ログインID"))
        ).Add(Sdx.Scaffold.Config.Item.Create()
          .Set("column", new Sdx.Scaffold.Config.Value("password"))
          .Set("label", new Sdx.Scaffold.Config.Value("パスワード"))
          .Set("setter", new Sdx.Scaffold.Config.Value("SetRawPassword"))
        ).Add(Sdx.Scaffold.Config.Item.Create()
          .Set("column", new Sdx.Scaffold.Config.Value("created_at"))
          .Set("label", new Sdx.Scaffold.Config.Value("登録日時"))
          //autoCurrentCheckboxは「現在日時で更新」のチェックボックスが付与され
          //チェックを入れて送信すると現在日時で更新されます。
          .Set("autoCurrentCheckbox", new Sdx.Scaffold.Config.Value("auto_created_at"))
        );

      return scaffold;
    }
  }
}
```

#### 並び替え

`area`テーブルは並び順を`sequence`というカラムに数字で保持しています。`area`の並び替えを実装するには`Scaffold.Manager.SortingOrder`を下記のように設定してくださ。

```c#
scaffold.SortingOrder
  .Set("column", new Sdx.Scaffold.Config.Value("sequence"))
  .Set("direction", new Sdx.Scaffold.Config.Value("DESC"));

  //`SortingOrder`は並び替えの機能がONになるだけです。並び順はあくまで`ListSelectHook`をつかって設定します。
  scaffold.ListSelectHook = new Config.Value("SelectDefaultOrder");
```


<br><br><br>
### カスタマイズ

#### フォーム要素のカスタマイズ

Scaffoldのフォーム要素はデフォルトで[`Sdx.Html.InputText`](../Html/InputText.cs)が生成されます。他の要素を作成したい場合は`Test.Orm.Table.Shop`に`Createキャメルケースカラム名Element`メソッドを作成してください。例えば`area_id`のフォーム要素を変更したい場合、下記のようなメソッドを作ります。

```c#
namespace Test.Orm.Table
{
  public class Shop : Sdx.Db.Table
  {
    ....

    //引数は下のどれでもOKです。オーバーロードがあった場合、下記の優先順位で探します。
    public static Sdx.Html.FormElement CreateAreaIdElement(Test.Orm.Shop shop, Sdx.Db.Connection conn)
    public static Sdx.Html.FormElement CreateAreaIdElement(Sdx.Db.Connection conn)
    public static Sdx.Html.FormElement CreateAreaIdElement()
    {
      var elem = new Sdx.Html.Select("area_id");

      var select = conn.Adapter.CreateSelect();
      select.AddFrom(new Test.Orm.Table.Area()).Table.SelectDefaultOrder(select, conn);
      select.ClearColumns().AddColumns("id", "name");

      elem.AddOption("", "場所を選択してください");
      conn.FetchKeyValuePairList<string, string>(select).ForEach(pair =>
      {
        elem.AddOption(pair);
      });

      return elem;
    }
  }
}
```

また、このメソッド名は`factory`属性を`Sdx.Scaffold.Config.Item`に追加することで変更可能です。

```c#
scaffold.FormList
  .Add(Sdx.Scaffold.Config.Item.Create()
    .Set("column", new Sdx.Scaffold.Config.Value("name"))
    .Set("label", new Sdx.Scaffold.Config.Value("店名"))
    .Set("factory", new Sdx.Scaffold.Config.Value("CreateFooBarElement"))
  )
```

`factory`はメソッド名の代わりに[MethodInfo](https://msdn.microsoft.com/ja-jp/library/system.reflection.methodinfo(v=vs.110).aspx)を渡すことも可能です。

#### バリデータのカスタマイズ

[TableMeta生成時](../Db/Record.md)にColumnにTypeを設定すると自動的にバリデータが生成され付与されます。どのようなバリデータが付与されるかは[Sdx.Db.Table](../Db/Table.cs)の`CreateValidatorList`メソッドを参照ください。

フォーム要素同様、バリデータも変更可能です。`Test.Orm.Table.Shop`に`Createキャメルケースカラム名Validators`を作成すると、自動バリデータは無視されそちらのメソッドが呼ばれます。


```c#
namespace Test.Orm.Table
{
  public class Shop : Sdx.Db.Table
  {
    ....
    //引数は下のどれでもOKです。オーバーロードがあった場合、下記の優先順位で探します。
    public static void CreateNameValidators(Sdx.Html.FormElement element, Sdx.Db.Record record, Sdx.Db.Connection conn)
    public static void CreateNameValidators(Sdx.Html.FormElement element, Sdx.Db.Connection conn)
    public static void CreateNameValidators(Sdx.Html.FormElement element)
    {
      //オートバリデータを使うことも可能。
      Meta.GetColumn("name").AppendValidators(element, record);
      element.AddValidator(new Sdx.Validation.StringLength(min: 3, max: 50));
    }
  }
}
```

バリデータ生成メソッドの名前を変更するには`validators`属性をConfigに付与してください。

```c#
scaffold.FormList
  .Add(Sdx.Scaffold.Config.Item.Create()
    .Set("column", new Sdx.Scaffold.Config.Value("name"))
    .Set("label", new Sdx.Scaffold.Config.Value("店名"))
    .Set("validators", new Sdx.Scaffold.Config.Value("CreateFooBarValidators"))
  )
```
