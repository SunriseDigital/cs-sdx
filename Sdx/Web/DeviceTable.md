# Sdx.Web.DeviceTable

## 概要
デバイス毎にURLが違う場合に、その対応表を管理します。<br>
URLがyaml形式の対応表に存在するかチェックをし、存在した場合各デバイスに対応したURLを取得できます。

## 使い方
下記のようなyamlファイルを用意します。

``` yaml
page:
  -
    query_match:
      tag: 1
    pc:
      url: /{area:(tokyo|kanagawa)}/shop/
      query:
          tag:  tg_prices_high
    sp:
      url: /sp/{area:(tokyo|kanagawa)}/shop/
      query:
          tag:  tg_high
    mb:
      url: /m/{area:(tokyo|kanagawa)}/shop/
      query:
          tag:  tg

  -
    pc:
      url: /{area:(tokyo|kanagawa)}/top/

    sp:
      url: /sp/{area:(tokyo|kanagawa)}/top/

    mb:
      url: /m/{area:(tokyo|kanagawa)}/top/
```

#### urlについて
- url内の{}は動的変数であることを示します。
{name: 正規表現}という書式で記述してください。

#### query_matchとqueryについて
- query_matchは対象のクエリーを完全一致させたい場合に記述します。<br>
省略した場合、queryは判定対象にせずに対応表にqueryがあってもなくてもマッチします。
- URLにクエリーがあるかないかだけ判断したい場合（完全一致させたくない場合）はquery_matchを空にしてください。
- 各デバイスでクエリーが共通の場合は、query_matchを`実際のクエリー名:マッチさせたい値`で設定してください。
- 各デバイスでクエリー名が別の場合は、query_match内に`適当なkey名:マッチさせたい値`、query内に`querymatchで設定した適当なkey名:実際のクエリー名`で設定する必要があります。


実際の使用例
http://○○/tokyo/shop?tg_prices_high=1&button=on
というURLにいると仮定します。ここからSP版のパスを取得したい場合は下記のように取得します。<br>
3つめの引数には設定ファイルのパスを指定してください。
```c#
Sdx.Web.DeviceTable.Current = new Sdx.Web.DeviceTable(Device.Pc, Request.RawUrl, "/tmp/path/");

deviceTable.GetUrl(Device.Sp);

```
返ってくるパスは
/sp/tokyo/shop?tg_high=1&button=on
です。
