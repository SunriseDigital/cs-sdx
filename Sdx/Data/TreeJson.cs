using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Web.Script.Serialization;

namespace Sdx.Data
{
  public class TreeJson : Tree
  {
    private dynamic BaseJson { get; set; }

    public override void Load(TextReader input)
    {
      var serializer = new JavaScriptSerializer();

      BaseJson = serializer.Deserialize<dynamic>(input.ReadToEnd());
    }

    public override string ToValue()
    {
      if (this.BaseJson == null)
      {
        throw new InvalidOperationException("Load before this.");
      }

      if (this.BaseJson.GetType() == typeof(Dictionary<string,object>))
      {
        throw new InvalidCastException("This is not String");
      }

      return this.BaseJson;
    }

    protected override List<Tree> ToList()
    {
      //if (this.BaseJson == null)
      //{
      //  throw new InvalidOperationException("Load before this.");
      //}

      //if (this.BaseJson.Type != JTokenType.Array)
      //{
      //  throw new InvalidCastException("Target is not List.");
      //}

      var list = new List<Tree>();
      //foreach (var item in this.BaseJson)
      //{
      //  var row = new TreeJson();
      //  row.BaseJson = item;
      //  list.Add(row);
      //}

      return list;
    }

    protected override Tree BuildTree(List<string> paths)
    {
      var TreeJson = new Sdx.Data.TreeJson();
      var target = this.BaseJson;
      var serializer = new JavaScriptSerializer();

      foreach (var item in paths)
      {
        if (BaseJson[item].GetType() == typeof(string)){
          BaseJson = BaseJson[item];
          break;
        }
        target = serializer.Deserialize<dynamic>(serializer.Serialize(BaseJson[item]));
        BaseJson = target;
      }

      TreeJson.BaseJson = BaseJson;
      return TreeJson;
    }

    protected override bool Exsits(List<string> paths)
    {
      //var target = this.BaseJson;

      //foreach (var item in paths)
      //{
      //  if (target.SelectToken(item) == null)
      //  {
      //    return false;
      //  }
      //  else
      //  {
      //    target = target.SelectToken(item);
      //  }
      //}
      
      return true;
    }
  }
}
