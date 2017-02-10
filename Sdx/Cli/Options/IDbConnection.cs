using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Cli.Options
{
  public interface IDbConnection
  {
    string ConfigPath { get; set; }

    string DbAdapterName { get; set; }
  }
}
