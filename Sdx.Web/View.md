# Sdx.Web.View

## 概要

テンプレート（MVCのView）で使う変数のホルダー。

## 使い方

スタティックプロパティーなのでどこでもセット・取得可能なので、UserControl上で参照することも可能です。`Vars`は`Sdx.Web.Holder`のインスタンスです。このインスタンスは`HttpContext.Current.Items`に保持せれているので、一回のリクエストで共有されるので注意してください。

### 変数のセット

`test.aspx.cs`
```c#
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class test : System.Web.UI.Page
{
  protected string stringValue;
  protected List<string> list;
  protected Dictionary<string, List<string>> listDic;

  protected void Page_Load(object sender, EventArgs e)
  {
    stringValue = "foobar";
    list = new List<string>() { "foo", "bar"  };

    listDic = new Dictionary<string, List<string>>(){
      {"first", new List<string>(){
        "first-value1",
        "first-value2",
        "first-value3"
      }},
      {"second", new List<string>(){
        "second-value1",
        "second-value2"
      }},
    };

    Sdx.Web.View.Vars.Set("stringValue", stringValue);
  }
}
```

### 値の取得

`Sdx.Web.Holder`には`object Get(string key)`と`T As<T>(string key)`の取得用メソッドが有ります。

`object Get(string key)`は存在しないキーで呼ぶとNULLが帰ります。`T As<T>(string key)`はキーが存在しない、あるいは`T`に変換できない場合、`T`のインスタンスを引数なしのコンストラクタで作成し返します。引数なしのコンストラクタが存在しない型の場合、例外が発生しますので適宜`Get`と使い分けてください。

`ContainsKey(string key)`メソッドでキーの存在をチェック可能です。

`test.aspx`
```asp
<%@ Page Language="C#" AutoEventWireup="true" CodeFile="index.aspx.cs" Inherits="index" %>
<%@ Register TagPrefix="uc" TagName="Header" Src="~\Header.ascx" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
      <div><%= Sdx.Web.View.Vars.As<string>("stringValue") %></div>
      <%
        //UserControlに変数を渡す
        Sdx.Web.View.Vars.Set("list", list);
        Sdx.Web.View.Vars.Set("listDic", listDic);
      %>
      <uc:Header id="Header" runat="server" />
      index
    </div>
    </form>
</body>
</html>
```

`Header.ascx`
```asp
<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Header.ascx.cs" Inherits="Header" %>
<h1>Header</h1>

<section>
  <h2></h2>
  <div><%= Sdx.Web.View.Vars.Get("stringValue") %></div>
</section>


<%if (Sdx.Web.View.Vars.ContainsKey("list")){ %>
<section>
  <h2>list</h2>
  <ul>
    <% foreach (var item in Sdx.Web.View.Vars.Get("list") as List<string>){ %>
    <li><%= item %></li>
    <% } %>
  </ul>
</section>
<%} %>

<section>
  <h2>As list</h2>
  <ul>
    <% foreach (var item in Sdx.Web.View.Vars.As<List<string>>("list"))
       { %>
    <li><%= item %></li>
    <% } %>
  </ul>
</section>


<%if (Sdx.Web.View.Vars.ContainsKey("listDic")){ %>
<section>
  <h2>listDic</h2>
  <ul>
    <% foreach (var row in Sdx.Web.View.Vars.Get("listDic") as Dictionary<string, List<string>>){ %>
    <li>
      <h3><%= row.Key %></h3>
      <ul>
        <%foreach (var item in row.Value){ %>
        <li><%= item %></li>
        <%} %>
      </ul>
    </li>
    <% } %>
  </ul>
</section>
<%} %>

<section>
  <h2>As listDic</h2>
  <ul>
    <% foreach (var row in Sdx.Web.View.Vars.As<Dictionary<string, List<string>>>("listDic")){ %>
    <li>
      <h3><%= row.Key %></h3>
      <ul>
        <%foreach (var item in row.Value){ %>
        <li><%= item %></li>
        <%} %>
      </ul>
    </li>
    <% } %>
  </ul>
</section>
```

