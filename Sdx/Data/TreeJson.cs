using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Web.Script.Serialization;
using System.Collections;

namespace Sdx.Data
{
  public class TreeJson : Tree
  {
    private dynamic BaseJson { get; set; }

    public override void Load(TextReader input)
    {
      var serializer = new JavaScriptSerializer();
      BaseJson = serializer.Deserialize<Dictionary<string, object>>(input.ReadToEnd());
    }

    public override string ToValue()
    {
      if (BaseJson == null)
      {
        throw new InvalidOperationException("Load before this.");
      }

      if (BaseJson is Dictionary<string, object>)
      {
        throw new InvalidCastException("This is not String");
      }

      return BaseJson.ToString();
    }

    protected override List<Tree> ToList()
    {
      if (BaseJson == null)
      {
        throw new InvalidOperationException("Load before this.");
      }

      if (!(BaseJson is ArrayList))
      {
        throw new InvalidCastException("Target is not List.");
      }
      
      var list = new List<Tree>();
      foreach (var item in BaseJson)
      {
        var row = new TreeJson();
        row.BaseJson = item;
        list.Add(row);
      }

      return list;
    }

    protected override Tree BuildTree(List<string> paths)
    {
      var TreeJson = new Sdx.Data.TreeJson();
      TreeJson.BaseJson = BaseJson;
      var serializer = new JavaScriptSerializer();

      foreach (var item in paths)
      {
        TreeJson.BaseJson = TreeJson.BaseJson[item];
      }

      return TreeJson;
    }

    protected override bool Exsits(List<string> paths)
    {
      var serializer = new JavaScriptSerializer();
      var target = BaseJson;

      foreach (var item in paths)
      {
        if(target is string){
          return false;
        }

        if (target.ContainsKey(item))
        {
          target = target[item];
        }
        else
        {
          return false;
        }
      }
      
      return true;
    }
  }
}
