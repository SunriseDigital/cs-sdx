using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Cli.Options
{
  public interface IDbConnection
  {
    bool UseAppSettings { get; set; }

    Db.AdapterType Type { get; set; }

    string ConfigPath { get; set; }

    string ConnectionName { get; set; }
  }
}
