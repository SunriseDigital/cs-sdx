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
