using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Sdx.Configuration
{
  public class DictionaryElement : ConfigurationElement
  {
    public IDictionary<string, string> Attributes { get; private set; }

    public DictionaryElement()
    {
      Attributes = new Dictionary<string, string>();
    }

    protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
    {
      Attributes.Add(name, value);
      return true;
    }

    public override int GetHashCode()
    {
      return Attributes.GetHashCode();
    }

    public override bool Equals(object compareTo)
    {
      var comp = compareTo as DictionaryElement;
      if(comp == null)
      {
        return false;
      }

      return Attributes.Equals(comp.Attributes);
    }
  }
}
