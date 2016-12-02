# Sdx.Web.DeviceTable

## 概要

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
- url内の{}は動的変数であることを示します。
{name: 正規表現}という書式で記述してください。

- query_matchは対象のクエリーを完全一致させたい場合に記述します。
省略した場合、queryは判定対象にせずに対応表にqueryがあってもなくてもマッチします。
URLにクエリーがあるかないかだけ判断したい場合（完全一致させたくない場合）はquery_matchを空にしてください。
query_matchのkeyとqueryのkeyは同じである必要があります。各デバイスでクエリーが共通の場合は実際のクエリー名で指定してください。


- 使用する際はweb.configに「Sdx.Web.DeviceTable.SettingFilePath」というキーで、用意したyamlファイルのパスを記述してください。


実際の使用例
http://○○/tokyo/shop?tg_prices_high=1&button=on
というURLにいると仮定します。ここからSP版のパスを取得したい場合は下記のように取得します。
```c#
var deviceTable = Sdx.Web.DeviceTable.Current;

deviceTable.GetUrl(Device.Sp);

```
返ってくるパスは
/tokyo/shop?tg_high=1&button=on
です。