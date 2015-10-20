# Sdx.Data.Tree

## 概要

YAMLなど木構造の設定ファイルのデータへアクセスを提供するための抽象クラスです。[Sdx.Config](../Config.md)で読み込むと便利です。

## 使い方


### データへのアクセス

下記のファイルが`/path/to/yaml.yml`に保存してあると仮定し解説します。

```yaml
string: 文字列
nested-dic:
  plane-string: 辞書内文字列
  inner-dic:
    key1:       value1
    key2:       value2
  inner-str-list:
    - listval1
    - listval2
```

[文字列]を取得するには下記のようにします。

```c#
var loader = new Sdx.Config<Sdx.Data.TreeYaml>();
loader.BaseDir = "/path/to/";

var str = loader.Get("yaml.yml").Get("string");
Console.WriteLine(str.Value) //文字列
```

`Get()`の返り値は新しい`Tree`のインスタンスです。`Value`プロパティで文字列データが取得可能です。

※ 指定したキーが辞書や配列だった場合、`InvalidCastException`が発生します。

返り値が`Tree`なので辞書データにも自由にアクセスが可能です。

```c#
var loader = new Sdx.Config<Sdx.Data.TreeYaml>();
loader.BaseDir = "/path/to/";

Console.WriteLine(loader.Get("yaml.yml").Get("nested-dic").Get("plane-string").Value);//辞書内文字列
Console.WriteLine(loader.Get("yaml.yml").Get("nested-dic").Get("inner-dic.key2").Value);//value2
```

### 配列へのアクセス

Treeは対象要素が配列の場合、foreachでの逐次処理とインデックスによるアクセスが可能です。どちらも返り値は`Tree`です。

```c#
var loader = new Sdx.Config<Sdx.Data.TreeYaml>();
loader.BaseDir = "/path/to/";

var list = loader.Get("yaml.yml").Get("nested-dic.inner-str-list");
foreach(var item in list)
{
  Console.WriteLine(item.Value);//listval1..listval2
}

Console.WriteLine(list[0].Value);
```