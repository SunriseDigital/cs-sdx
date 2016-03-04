using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sdx.Scaffold.Config
{
  public class Item : IEnumerable<KeyValuePair<string, Value>>
  {
    private enum DisplayType
    {
      COLUMN,
      DYNAMIC,
      HTML,
    }

    private DisplayType? type;
    
    public static Item Create()
    {
      return new Item();
    }

    private Dictionary<string, Value> vars = new Dictionary<string, Value>();

    private static Regex htmlRegex = new Regex(@"\{([^}]+)\}");

    public bool StrictCheck = true;

    //こいつをつけないとコンパイルできない。
    //http://tyheeeee.hateblo.jp/entry/2014/01/25/C%23%E3%81%AB%E3%81%8A%E3%81%91%E3%82%8BIndexer%E8%A9%B3%E8%AA%AC%EF%BC%88%E3%83%9D%E3%83%AD%E3%83%AA%E3%82%82%E3%81%82%E3%82%8B%E3%82%88%EF%BC%81%EF%BC%89
    [System.Runtime.CompilerServices.IndexerName("ItemIndexer")]
    public Value this[string key]
    {
      set
      {
        switch(key)
        {
          case "column":
            type = DisplayType.COLUMN;
            break;
          case "dynamic":
            type = DisplayType.DYNAMIC;
            break;
          case "html":
            type = DisplayType.HTML;
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

    public Item Set(string key, Value value)
    {
      this[key] = value;
      return this;
    }

    public Value Get(string key)
    {
      return this.vars[key];
    }

    public IEnumerator<KeyValuePair<string, Value>> GetEnumerator()
    {
      return ((IEnumerable<KeyValuePair<string, Value>>)vars).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable<KeyValuePair<string, Value>>)vars).GetEnumerator();
    }


    public string Display(Db.Record record, Db.Connection conn)
    {
      if(type == null)
      {
        throw new InvalidOperationException("Missing param type, Please set the one of the 'column|dynamic|html'");
      }

      switch (type)
      {
        case DisplayType.COLUMN:
          return this.BuildColumn(record);
        case DisplayType.DYNAMIC:
          return this.BuildDynamic(record, conn);
        case DisplayType.HTML:
          return this.BuildHtml(record);
      }

      throw new NotImplementedException("Not implemented the param type " + type);
    }

    private string BuildHtml(Db.Record record)
    {
      var replaced = new Dictionary<string, bool>();

      var html = Get("html").ToString();
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
      return record.Get<string>(Get("dynamic").ToString(), conn);
    }

    private string BuildColumn(Db.Record record)
    {
      return record.GetString(Get("column").ToString());
    }

    internal bool Is(string key)
    {
      if(!this.ContainsKey(key))
      {
        return false;
      }

      return this[key].ToBool();
    }
  }
}
