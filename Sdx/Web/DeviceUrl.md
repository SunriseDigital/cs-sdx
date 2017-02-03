# Sdx.Web.DeviceUrl

## 概要

PC・スマホ・ガラケーのURLが違う場合に、`alternate`タグや`canonical`タグを出す時のヘルパーです。URLから動的変数をプレイスホルダにバインドしたり、クエリーのキー名が違う場合にも対応可能です。

※[DeviceTable.md](DeviceTable)

## 使い方


### 単純なURL
現在のページのデバイス、他デバイスのURLフォーマットを与えて初期化します。URLフォーマットは名前付き引数を使うと便利です。

下記のようなURLで、PCページで利用するコードです。
| デバイス | URL |
|:--|:--|
| PC | /top.aspx?foo=bar |
| スマホ | /sp/top.aspx?foo=bar |
| ガラケー | /m/top.aspx?foo=bar |

```c#
var deviceUrl = new Sdx.Web.DeviceUrl(
  Sdx.Web.DeviceUrl.Device.Pc,
  sp: "/sp/top.aspx",
  mb: "/i/top.aspx"
);
```

出力は下記のプロパティで行います。各プロパティの返り値は[Sdx.Web.Url](Sdx.Web.Url.md)です。クエリは現在のものがそのまま付与されます。

```c#
// PCはURLフォーマットを与えていませんが現在のパスから取得されます。
deviceUrl.Pc.Build();
// /top.aspx?foo=bar


deviceUrl.Sp.Build();
// /sp/top.aspx?foo=bar

deviceUrl.Mb.Build();
// /m/top.aspx?foo=bar
```

### クエリを変える

クエリ名を変える場合は初期化時に対応表をセットしておきます。

```c#
var deviceUrl = new Sdx.Web.DeviceUrl(
  Sdx.Web.DeviceUrl.Device.Pc,
  sp: "/sp/top.aspx",
  mb: "/i/top.aspx"
);

deviceUrl.AddSpQueryMap("foo", "sp_foo");
```

```c#
deviceUrl.Sp.Build();
// /sp/top.aspx?sp_foo=bar
```

クエリーを追加、削除したい場合はUrlの機能を利用してください。

```c#
deviceUrl.Sp.Remove("sp_foo").Build();
// /sp/top.aspx
```

`next`タグを出力する場合はUrl.Next()を使うと便利です。

```c#
deviceUrl.Pc.Next("pid").Build();
// /top.aspx?foo=bar&pid=2
```

### 動的なパス

| デバイス | URL |
|:--|:--|
| PC | /tokyo/A1301/A130102/ |
| スマホ | /sp/tokyo/A1301/A130102/ |
| ガラケー | /m/tokyo/A1301/A130102/ |

動的なパスは正規表現を使って抽出可能です。

```c#
var deviceUrl = new Sdx.Web.DeviceUrl(
  Sdx.Web.DeviceUrl.Device.Pc,
  sp: "/sp/{1}/{2}/{3}/",
  mb: "/i/{1}/{2}/{3}/"
  regex: "^/([^/]+)/(A[0-9]+)/(A[0-9]+)/"
);
```

URLフォーマットのプレイスフォルダの数字は正規表現のグループキャプチャの番号です。0はマッチした全体（今回の例では`/tokyo/A1301/A130102/`）が入っているので`1`からになります。

```
deviceUrl.Sp.Build();
// /sp/tokyo/A1301/A130102/

deviceUrl.Mb.Build();
// /m/tokyo/A1301/A130102/
```
