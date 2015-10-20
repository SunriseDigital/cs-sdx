# Sdx.Config

## 概要

特定のディレクトリから木構造のファイルを読み込み[Sdx.Data.Tree](Data/Tree.md)を生成します。一度読み込んだデータはメモリにキャッシュされます。

## 使い方

```c#
var loader = new Sdx.Config<Sdx.Data.TreeYaml>();
loader.BaseDir = "/path/to/config/dir";

Sdx.Data.Tree config = loader.Get("test.yml");
```