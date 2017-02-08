using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Cli.Options
{
  public class Db
  {
    public enum AdapterType
    {
      sqlserver,
      mysql
    }

    public static Sdx.Db.Adapter.Base CreateAdapter(IDbConnection options)
    {
      var connectionString = DeletectConnectionString(options);
      if(connectionString == null)
      {
        throw new ArgumentException(string.Format(
          "Missing {0} connection setting in {1} at {2}",
          options.ConnectionName,
          options.ConfigPath,
          options.UseAppSettings ? "AppSettings" : "ConnectionStrings"
        ));
      }

      return CreateDbAdapter(connectionString, options.Type);
    }

    private static Sdx.Db.Adapter.Base CreateDbAdapter(string connectionString, AdapterType adapterType)
    {
      Sdx.Db.Adapter.Base db = null;
      switch (adapterType)
      {
        case AdapterType.sqlserver:
          db = new Sdx.Db.Adapter.SqlServer();
          break;
        case AdapterType.mysql:
          db = new Sdx.Db.Adapter.MySql();
          break;
        default:
          throw new ArgumentException("Illegal db adapter type " + adapterType + " specified.");
      }

      db.ConnectionString = connectionString;

      return db;
    }

    private static string DeletectConnectionString(IDbConnection options)
    {
      //configを読み込む
      ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();

      fileMap.ExeConfigFilename = options.ConfigPath;
      System.Configuration.Configuration config =
          ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

      string connectionString = null;
      if (options.UseAppSettings)
      {
        var kv = config.AppSettings.Settings[options.ConnectionName];
        if (kv != null)
        {
          connectionString = kv.Value;
        }
      }
      else
      {
        var kv = config.ConnectionStrings.ConnectionStrings[options.ConnectionName];
        if (kv != null)
        {
          connectionString = kv.ConnectionString;
        }
      }

      return connectionString;
    }
  }
}
