# Sdx.Web.DeviceTable

## 概要


※こちらのクラスはHttpModuleを入れた全ページで評価されるため非効率なので、[DeviceUrl.md](DeviceUrl)を使ってください。

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
- query_matchは対象のクエリーを値まで一致で検索したい場合に記述します。<br>
省略されたクエリーは検索対象にはならず、あってもなくてもマッチします。（部分一致）
- URLにクエリーがあるかないかだけ判断したい場合（完全一致させたくない場合）はquery_matchをキーのみの値を空で記述してください。
- 各デバイスでクエリーが共通の場合は、query_matchを`実際のクエリー名:マッチさせたい値`で記述してください。
- 各デバイスでクエリー名が別の場合は、query_match内に`適当なkey名:マッチさせたい値`、query内に`querymatchで記述した適当なkey名:実際のクエリー名`で記述する必要があります。

#### query_match_perfectについて
- query_match_perfectが指定されていた場合は、クエリーのキーを完全一致で検索します。
- query_matchとの併用はできません。
- query_match_perfectの中身が空の場合は現在のURLにクエリーがあった場合マッチしません。

#### exclude_build_queryについて
- GetUrlをした際に、特定のクエリーを除外したURLが欲しい場合に配列で記述します。<br>
exclude_build_queryはURLの一致判定には影響せず、URLを組み立てる際にクエリの除外を判断するだけのキーになります。
``` yaml
page:
  -
    query_match:
      tag: 1
    exclude_build_query: [foo]
    pc:
      url: /{area:(tokyo|kanagawa)}/shop/
      query:
          tag:  tg_prices_high
      exclude_build_query: [bar]
    sp:
      url: /sp/{area:(tokyo|kanagawa)}/shop/
      query:
          tag:  tg_high
    mb:
      url: /m/{area:(tokyo|kanagawa)}/shop/
      query:
          tag:  tg
```
上記のような記述の場合は、PCのURLを組み立てる際には「foo,bar」が除外され、その他のデバイスの場合は「foo」のみが除外されます。


実際の使用例
http://○○/tokyo/shop?tg_prices_high=1&button=on
というURLにいると仮定します。ここからSP版のパスを取得したい場合は下記のように取得します。<br>
3つめの引数には設定ファイルのパスを指定してください。
```c#
Sdx.Web.DeviceTable.Current = new Sdx.Web.DeviceTable(Device.Pc, Request.RawUrl, "/tmp/path/");

deviceTable.GetUrl(Device.Sp);

```
指定したyamlファイルに先ほどの設定がかかれていた場合、返ってくるURLは
/sp/tokyo/shop?tg_high=1&button=on
です。
