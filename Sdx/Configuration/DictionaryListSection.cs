using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Sdx.Configuration
{
  public class DictionaryListSection : ConfigurationSection
  {
    [ConfigurationProperty("Items", IsDefaultCollection = false)]
    [ConfigurationCollection(typeof(Configuration.ElementCollection<Configuration.DictionaryElement>),
        AddItemName = "add",
        ClearItemsName = "clear",
        RemoveItemName = "remove")]
    public Configuration.ElementCollection<Configuration.DictionaryElement> Items
    {
      get
      {
        return (Configuration.ElementCollection<Configuration.DictionaryElement>)base["Items"];
      }
    }
  }
}
