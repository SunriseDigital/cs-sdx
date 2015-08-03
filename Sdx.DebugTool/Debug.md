# Debug

## 概要

変数のダンプ、ページヘのデバッグ用ログの出力を行います。有効にするには`Sdx.DebugTool.DisplayHttpModule`をアプリケーションに追加する必要があります。詳しくは[こちら](Sdx.DebugTool/DisplayHttpModule)をご覧ください。

## メソッド

### String Dump(Object value)

変数の中身を見やすい形で出力します。

```c#
Sdx.DebugTool.Debug.Dump(someValue);
```

### void Log(Object value, String title = "")

ページヘデバッグ用ログを出力します。タイトル部分に表示される`[0.303/4.978]`は`直前のLogからの経過秒/リクエスト開始からの経過秒`です。

```c#
Sdx.DebugTool.Debug.Log(someValue, "someValue");
```
