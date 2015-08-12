# DisplayHttpModule

## 概要

[Sdx.DebugTool.Debug.Log(someValue)](https://github.com/SunriseDigital/cs-sdx/blob/master/Sdx.DebugTool/Debug.md#void-logobject-value-string-title--)の値をページに付与するHttpModuleです。

## 有効にする

有効にするには、Web.configにHttpModuleの設定を行います。

IIS6以前（クラシック）
```xml
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
  <system.web>
    <httpModules>
      <add name="DisplayHttpModule" type="Sdx.DebugTool.DisplayHttpModule"/>
    </httpModules>
  </system.web>
</configuration>
```

IIS7
```xml
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
  <system.webServer>
    <modules>
      <add name="DisplayHttpModule" type="Sdx.DebugTool.DisplayHttpModule"/>
    </modules>
  </system.webServer>
</configuration>
```
