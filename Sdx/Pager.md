# Sdx.Pager

## 概要

ページネーションを実装する機能を提供します。`Sdx.Pager`自体は主に「次のページがあるか？」「前のページはあるか？」という情報を管理するクラスでHTMLのAタグのレンダリングは`Sdx.Html.PagerLink`クラスで行います。

## 使い方

### `Sdx.Db.Sql.Select`と一緒に使う

#### cs

```c#
//1ページの数と総カウント数を使用して初期化します。
var pager = new Sdx.Pager(50, conn.CountRow(select));

//PagerLinkを生成。
var pagerLink = new Sdx.Html.PagerLink(pager);

//URLのパラメータ名。
//PagerLinkを使う場合、現在ページデータはこのパラメータから取得して自動で`Pager`にセットします。
//デフォルト値は`page`です。`page`で問題なければセットする必要はありません。
//pagerLink.VarName = "pid";

//selectにPagerを利用してLIMIT/OFFSETを付与
//現在ページがセットされていないと正しくセットできないため例外になります。
select.LimitPager(pager);
```

#### aspx側

```c#
<div class="row">
  <div class="text-center">
    <%= pagerLink.GetFisrt().AddText("|<").Render() %>
    <%= pagerLink.GetPrev().AddText("<").Render() %>
    <span class="page-number">
      <%=pagerLink.Pager.Page %>&nbsp;/&nbsp;<%=pagerLink.Pager.LastPage %>
    </span>
    <%= pagerLink.GetNext().AddText(">").Render() %>
    <%= pagerLink.GetLast().AddText(">|").Render() %>
  </div>
</div>
```

### 番号付きのページャー
こういうタイプのページングを生成する例です
```
[←前へ][1][2][3][4][5][次へ→]
```

#### aspx記述例
CSファイル側は前項と同じ記述でOKです。
```asp
<div class="row">
  <div class="text-center">
    <%= pagerLink.GetPrev().AddText("←前へ").Render() %>
    
    <div class="link-list">
      <%-- GetLinksTag の引数分だけリンクタグが生成されます  --%>
      <% foreach(var link in pager_link.GetLinksTag(5)) { %>
        <%= link.Render() %>
      <% } %>
    </div>

    <%= pagerLink.GetNext().AddText("次へ→").Render() %>
  </div>
</div>
```

#### aspx記述例(2)
`GetLinksTag()` の引数にコールバックを渡すこともできます。
aタグの中身を数字以外にしたい場合などに使えます。
下記は画像をaタグの中に表示させたい場合の例です。
```asp
<div class="link-list">
  <% foreach(var link in pager_link.GetLinksTag(5, page => {string.Format(@"<img src='/path/to/img/{0}.jpg alt='{0}' />", page)})) { %>
    <%= link.Render() %>
  <% } %>
</div>
```
