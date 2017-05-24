# IISのサイトを複製します。サイト単体の設定だけでなくapplicationHost.configのlocationもコピーします。
param([string]$hostName, [string]$adminPwd, [string]$fromIISName, [string]$toIISName)


if(($hostName -eq "") -Or ($adminPwd -eq "") -Or ($fromIISName -eq "") -Or ($toIISName -eq "")){
  Write-Host "Illegal arguments" -foregroundcolor "red"
  Write-Host "arg0: IP address of the destination server"
  Write-Host "arg1: Administrator password of target server"
  Write-Host "arg2: Source site name"
  Write-Host "arg3: Destination site name"
  Write-Host "Ex: copyIISSite.ps1 0.0.0.0 *** sitename sitename2"
  exit 1
}

$username = 'Administrator'
$pass = ConvertTo-SecureString -AsPlainText $adminPwd -Force
$cred = New-Object System.Management.Automation.PSCredential -ArgumentList $username,$pass

Invoke-Command -ComputerName $hostName -Credential $cred -ScriptBlock {
  $fromIISName = $args[0]
  $toIISName = $args[1]

  # 一度だけ「別のプロセスで使用されているため、保存できません」のようなエラーで保存に失敗したが、再現することができなかった。
  # とりえあず、tryで囲ってリトライするように実装してみたがそのエラーがtryでキャッチできるか不明です。
  #
  # ちなみに別プロセスでファイルをFileShare.Noneでロックしてみたが、ロック解除まで待たされるだけで例外にならなかった。
  $saveRetry = 0
  function saveIISConfig($iisConfig, $iisConfigPath, $toIISName){
    try{
      $iisConfig.save($iisConfigPath)
    } catch [Exception]{
      if($saveRetry -lt 5){
        $sec = 3
        Write-Host "Fail to save by '$($error[0])'. I will try to save again after ${sec} second..." -foregroundcolor "red"
        Start-Sleep -s $sec
        ++$saveRetry
        saveIISConfig $iisConfig $iisConfigPath $toIISName
      } else {
        Write-Error "Fail to save by '$($error[0])'."
        cmd /c "$Env:Windir\System32\inetsrv\appcmd delete site ""$toIISName"""
      }
    }
  }

  #コピー元があるかチェック
  $exists = cmd /c "$Env:Windir\System32\inetsrv\appcmd list site /site.name:""${fromIISName}"""
  if($exists.Length -eq 0){
    Write-Host "Not exists ${fromIISName}" -foregroundcolor "red"
    exit 1
  }

  #コピー先がないかチェック
  $exists = cmd /c "$Env:Windir\System32\inetsrv\appcmd list site /site.name:""${toIISName}"""
  if($exists.Length -ne 0){
    Write-Host "Already exists ${toIISName}" -foregroundcolor "red"
    exit 1
  }

  $date = Get-Date -Format "yyyyMMdd_HHmmss_fff"
  $xmlPath = "${env:userprofile}\${fromIISName}_${date}.xml"

  # いったんxmlで書き出す
  cmd /c "$Env:Windir\System32\inetsrv\appcmd list site ""$fromIISName"" /config /xml" | Set-Content $xmlPath

  # IDを消して名前を変更する
  $siteConfig = [xml](Get-Content $xmlPath)
  $siteConfig.appcmd.SITE.SetAttribute("SITE.NAME", $toIISName)
  $siteConfig.appcmd.SITE.site.SetAttribute("name", $toIISName)
  $siteConfig.appcmd.SITE.RemoveAttribute("SITE.ID")
  $siteConfig.appcmd.SITE.site.RemoveAttribute("id")
  $siteConfig.save($xmlPath)

  # サイトを登録する
  Get-Content $xmlPath | cmd /c "$Env:Windir\System32\inetsrv\appcmd add site /in" | Write-Host -foregroundcolor "green"

  # applicationHost.configのlocationを複製する。
  $iisConfigPath = "$Env:Windir\System32\inetsrv\config\applicationHost.config"
  $iisConfig = [xml](Get-Content $iisConfigPath)
  $updateCount = 0

  $iisConfig.configuration.location | where {($_.path -eq "$fromIISName") -Or ($_.path -like "$fromIISName/*")} | foreach {
    $location = $_
    $toPath = $location.path -replace "^$fromIISName(.*)", ($toIISName + '$1')
    #既に対象のlocationがあったら落とす。基本的にはsiteが無かったのであるはずない。同じ名前のlocationが二つあるとIISが死ぬので防御的措置
    if(($iisConfig.configuration.location | where {$_.path -eq $toPath}) -ne $null){
      Write-Host "$toPath location already exists !" -foregroundcolor "red"
      cmd /c "$Env:Windir\System32\inetsrv\appcmd delete site ""$toIISName"""
      exit 1
    }

    $newXml = [xml]$location.OuterXml
    $newLocation = $iisConfig.ImportNode($newXml.location, $true)
    $newLocation.path = $toPath

    $iisConfig.configuration.AppendChild($newLocation) | Out-Null
    Write-Host "Add location[path=${toPath}] to ${iisConfigPath}" -foregroundcolor "green"
    ++$updateCount
  }

  if($updateCount -gt 0){
    saveIISConfig $iisConfig $iisConfigPath $toIISName
  }

  # 書き出したxmlは掃除。
  Remove-Item $xmlPath
} -argumentlist $fromIISName,$toIISName
