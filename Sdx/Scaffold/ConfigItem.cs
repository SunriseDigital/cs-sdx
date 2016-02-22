using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sdx.Scaffold
{
  public class ConfigItem : IEnumerable<KeyValuePair<string, string>>
  {
    public enum Type
    {
      COLUMN,
      DYNAMIC,
      HTML,
    }

    private Type? type;
    
    public static ConfigItem Create()
    {
      return new ConfigItem();
    }

    private Dictionary<string, ConfigValue> vars = new Dictionary<string, ConfigValue>();

    private static Regex htmlRegex = new Regex(@"\{([^}]+)\}");

    public bool StrictCheck = true;

    public ConfigValue this[string key]
    {
      set
      {
        switch(key)
        {
          case "column":
            type = Type.COLUMN;
            break;
          case "dynamic":
            type = Type.DYNAMIC;
            break;
          case "html":
            type = Type.HTML;
            break;
        }

        this.vars[key] = value;
      }

      get
      {
        return this.vars[key];
      }
    }

    public bool ContainsKey(string key)
    {
      return this.vars.ContainsKey(key);
    }

    public ConfigItem Set(string key, ConfigValue value)
    {
      this[key] = value;
      return this;
    }

    public ConfigValue Get(string key)
    {
      return this.vars[key];
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
      return ((IEnumerable<KeyValuePair<string, string>>)vars).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable<KeyValuePair<string, string>>)vars).GetEnumerator();
    }


    public string Build(Db.Record record, Db.Connection conn)
    {
      if(type == null)
      {
        throw new InvalidOperationException("Missing param type, Please set the one of the 'column|dynamic|html'");
      }

      switch (type)
      {
        case Type.COLUMN:
          return this.BuildColumn(record);
        case Type.DYNAMIC:
          return this.BuildDynamic(record, conn);
        case Type.HTML:
          return this.BuildHtml(record);
      }

      throw new NotImplementedException("Not implemented the param type " + type);
    }

    private string BuildHtml(Db.Record record)
    {
      var replaced = new Dictionary<string, bool>();

      var html = Get("html").Value;
      var match = htmlRegex.Match(html);

      while(match.Success)
      {
        var search = match.Groups[0].Value;
        var path = match.Groups[1].Value;
        if (!replaced.ContainsKey(search))
        {
          replaced[search] = true;
          html = html.Replace(search, record.Get(path).ToString());
        }
        
        match = match.NextMatch();
      }


      return html;
    }

    private string BuildDynamic(Db.Record record, Db.Connection conn)
    {
      return record.Get<string>(Get("dynamic").Value, conn);
    }

    private string BuildColumn(Db.Record record)
    {
      return record.GetString(Get("column").Value);
    }
  }
}
