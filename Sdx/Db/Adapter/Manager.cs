using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Configuration;

namespace Sdx.Db.Adapter
{
  /// <summary>
  /// DB負荷分散用Class
  /// 複数のAdapterをセットしておきランダムで取得する。
  /// </summary>
  public class Manager
  {
    private List<Base> writeAdapters = new List<Base>();
    private List<Base> readAdapters = new List<Base>();

    private static Dictionary<string, Object> managerDic = new Dictionary<string, Object>();

    private static Random random = new Random();

    public void AddCommonAdapter(Base adapter)
    {
      writeAdapters.Add(adapter);
      readAdapters.Add(adapter);
    }

    public void AddReadAdapter(Base adapter)
    {
      readAdapters.Add(adapter);
    }

    public void AddWriteAdapter(Base adapter)
    {
      writeAdapters.Add(adapter);
    }

    private static Base GetRandom(List<Base> list)
    {
      if (list.Count == 0)
      {
        throw new InvalidOperationException("Adapter list is empty.");
      }
      else if (list.Count == 1)
      {
        return list[0];
      }
      else
      {
        //Random.Next(maxValue)はmaxValueを含みません
        return list[random.Next(list.Count)];
      }
    }

    public Base Read
    {
      get
      {
        return Manager.GetRandom(readAdapters);
      }
    }

    public Base Write
    {
      get
      {
        return Manager.GetRandom(writeAdapters);
      }
    }

    public static void Add(string key, Manager manager)
    {
      managerDic[key] = manager;
    }

    public static void Add(string key, Func<Manager> getter)
    {
      managerDic[key] = getter;
    }

    public static bool Has(string key)
    {
      return managerDic.ContainsKey(key);
    }

    public static Manager Get(string key)
    {
      var target = managerDic[key];
      if (target is Manager)
      {
        return (Manager)target;
      }
      else if(target is Func<Manager>)
      {
        return ((Func<Manager>) target)();
      }
      else
      {
        throw new InvalidOperationException("Invalid type " + target.GetType());
      }
    }

    /// <summary>
    /// System.Configuration.Configuration|ConfigurationManager|WebConfigurationManager全てから生成できるようにするため、ややこしい構造になっています。
    /// というか、MSふざけんな。
    /// </summary>
    /// <param name="setting"></param>
    /// <param name="connectionString"></param>
    private static void Add(IDictionary<string, string> setting, string connectionString)
    {
      Manager manager = null;
      var name = setting["name"];
      if (Has(name))
      {
        manager = Get(name);
      }
      else
      {
        manager = new Manager();
        Add(name, manager);
      }

      var db = (Db.Adapter.Base)Activator.CreateInstance(Type.GetType(setting["adapterClass"]));
      db.ConnectionString = connectionString;

      switch(setting["connectionType"])
      {
        case "common":
          manager.AddCommonAdapter(db);
          break;
        case "read":
          manager.AddReadAdapter(db);
          break;
        case "write":
          manager.AddWriteAdapter(db);
          break;
        default:
          throw new InvalidOperationException("Illegal connectionType " + setting["connectionType"] + " [common|read|write]");
      }
    }

    public static void Add(IDictionary<string, string> setting, System.Configuration.ConnectionStringSettingsCollection connectionStrings, System.Collections.Specialized.NameValueCollection appSettings)
    {
      if (setting.ContainsKey("alias"))
      {
        AddAlias(setting["name"], setting["alias"]);
        return;
      }

      string connectionString;
      if (setting["configType"] == "connectionStrings")
      {
        connectionString = connectionStrings[setting["configName"]].ConnectionString;
      }
      else if (setting["configType"] == "appSettings")
      {
        connectionString = appSettings[setting["configName"]];
      }
      else
      {
        throw new InvalidOperationException("Missing or invalid configType " + setting["configName"] + " [connectionStrings|appSettings]");
      }

      Add(setting, connectionString);
    }

    internal static void Add(Dictionary<string, string> setting, System.Configuration.ConnectionStringSettingsCollection connectionStrings, System.Configuration.AppSettingsSection appSettings)
    {
      if(setting.ContainsKey("alias"))
      {
        AddAlias(setting["name"], setting["alias"]);
        return;
      }

      string connectionString;
      if (setting["configType"] == "connectionStrings")
      {
        connectionString = connectionStrings[setting["configName"]].ConnectionString;
      }
      else if (setting["configType"] == "appSettings")
      {
        connectionString = appSettings.Settings[setting["configName"]].Value;
      }
      else
      {
        throw new InvalidOperationException("Missing or invalid configType " + setting["configName"] + " [connectionStrings|appSettings]");
      }

      Add(setting, connectionString);
    }

    private static void AddAlias(string name, string aliasName)
    {
      var manager = Get(aliasName);
      Add(name, manager);
    }
  }
}
