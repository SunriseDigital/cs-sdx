using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace Sdx.Web
{
  public class DeviceTable
  {
    private string query { get; set; }

    private string url { get; set; }

    private string queryMatch { get; set; }

    public enum Device
    {
      Pc,
      Sp,
      Mb
    }

    public DeviceTable(string pageYaml)
    {
      foreach(var ){

      }
    }

    public static Sdx.Web.DeviceTable Current 
    {
      get
      {
        if(!Sdx.Context.Current.Vars.ContainsKey("Sdx.Web.DeviceTable.Current"))
        {
          Sdx.Context.Current.Vars["Sdx.Web.DeviceTable.Current"] = CreateCurrent();
        }

        return (Sdx.Web.DeviceTable)Sdx.Context.Current.Vars["Sdx.Web.DeviceTable.Current"];
      }
    }

    private static Sdx.Web.DeviceTable CreateCurrent()
    {
      var filePath = WebConfigurationManager.AppSettings["Sdx.Web.DeviceTable.SettingFilePath"];
      //if(filePath == null){
        
      //}

      //ファイル読み込む
      var fs = new FileStream(filePath, FileMode.Open);
      var input = new StreamReader(fs, Encoding.GetEncoding("utf-8"));

      //decode
      var yamlSettings = Sdx.Util.Yaml.Decode<List<string>>(input.ReadToEnd());

      foreach (var pageYaml in yamlSettings)
      {
        var deviceTable = new Sdx.Web.DeviceTable(pageYaml);
        if (deviceTable.IsMatch(Device.Pc, HttpContext.Current.Request.Url.AbsolutePath))
        {
          return deviceTable;
        }
      }

      return null;
    }

    private bool IsMatch(Device device, string url)
    {
      return false;
    }

    public string GetUrl(Device device)
    {
      return "";
    }
  }
}
