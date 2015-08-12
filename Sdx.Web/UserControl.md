# Sdx.Web.UserControl

## 概要

インクルードに使うUserControlに親ページから変数を渡す仕組みを提供します。

## 使い方

### UserControlの準備

変数のやり取りは`Sdx.Web.UserControl.Vars`プロパティを介して行いますので、`Sdx.Web.UserControl`を継承して作成します。`Vars`プロパティは`Sdx.Web.Holder`のインスタンスです。

TestInclude.ascx.cs
```c#
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class TestInclude : Sdx.Web.UserControl
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }
}
```

### 親ページ

`Sdx.Web.UserControl.Find(Page page, string controlId)`でそのページで使用しているUserControlを取得可能です。`Vars`プロパティの`Set(string key, object value)`メソッドで変数をセットします。

test.aspx.cs
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
  }
}
```

test.aspx
```c#
<%@ Page Language="C#" AutoEventWireup="true" CodeFile="test.aspx.cs" Inherits="test" %>
<%@ Register TagPrefix="uc" TagName="TestInclude" Src="~\TestInclude.ascx" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
  <% Sdx.Web.UserControl.Find(Page, "TestInclude1").Vars.Set("list", list); %>
  <% Sdx.Web.UserControl.Find(Page, "TestInclude1").Vars.Set("listDic", listDic); %>
  <% Sdx.Web.UserControl.Find(Page, "TestInclude2").Vars.Set("stringValue", stringValue); %>

  <uc:TestInclude id="TestInclude1" runat="server" />
  <uc:TestInclude id="TestInclude2" runat="server" />
</body>
</html>
```

### UserControlで値を取得

`Sdx.Web.Holder`には`object Get(string key)`と`T As<T>(string key)`の取得用メソッドが有ります。

`object Get(string key)`は存在しないキーで呼ぶとNULLが帰ります。`T As<T>(string key)`はキーが存在しない、あるいは`T`に変換できない場合、`T`のインスタンスを引数なしのコンストラクタで作成し返します。引数なしのコンストラクタが存在しない型の場合、例外が発生しますので適宜`Get`と使い分けてください。

`ContainsKey(string key)`メソッドでキーの存在をチェック可能です。

```c#
<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TestInclude.ascx.cs" Inherits="TestInclude" %>
<h1>TestInclude</h1>

<section>
  <h2></h2>
  <div><%= Vars.Get("stringValue") %></div>
</section>


<%if (Vars.ContainsKey("list")){ %>
<section>
  <h2>list</h2>
  <ul>
    <% foreach (var item in Vars.Get("list") as List<string>){ %>
    <li><%= item %></li>
    <% } %>
  </ul>
</section>
<%} %>

<section>
  <h2>As list</h2>
  <ul>
    <% foreach (var item in Vars.As<List<string>>("list"))
       { %>
    <li><%= item %></li>
    <% } %>
  </ul>
</section>


<%if (Vars.ContainsKey("listDic")){ %>
<section>
  <h2>listDic</h2>
  <ul>
    <% foreach (var row in Vars.Get("listDic") as Dictionary<string, List<string>>){ %>
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
    <% foreach (var row in Vars.As<Dictionary<string, List<string>>>("listDic")){ %>
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

