using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Sdx.Configuration
{
  public class DictionaryListSection : ConfigurationSection
  {
    [ConfigurationProperty("List", IsDefaultCollection = false)]
    [ConfigurationCollection(typeof(Configuration.ElementCollection<Configuration.DictionaryElement>),
        AddItemName = "add",
        ClearItemsName = "clear",
        RemoveItemName = "remove")]
    public Configuration.ElementCollection<Configuration.DictionaryElement> List
    {
      get
      {
        return (Configuration.ElementCollection<Configuration.DictionaryElement>)base["List"];
      }
    }
  }
}
