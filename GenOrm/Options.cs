using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandLine;
using CommandLine.Text;

namespace GenOrm
{
  class Options : Sdx.Cli.Options.IDbConnection
  {
    [Value(0, Required = true, HelpText = "Path to config file.")]
    public string ConfigPath { get; set; }

    [Value(1, Required = true, HelpText = "Db Adapter name.")]
    public string DbAdapterName { get; set; }

    [Value(2, Required = true, HelpText = "Base directory.")]
    public string BaseDir { get; set; }

    [Value(3, Required = true, HelpText = "Namespace.")]
    public string Namespace { get; set; }

    [Value(4, Required = false, HelpText = "Table names with comma separator.")]
    public string TableName { get; set; }

    public IEnumerable<string> TableNames
    {
      get
      {
        if(TableName != null)
        {
          return TableName.Split(',');
        }
        else
        {
          return new string[0];
        }
      }
    }
  }
}
