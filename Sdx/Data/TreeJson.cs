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
    public dynamic BaseJson { get; set; }

    public override void Load(TextReader input)
    {
      var serializer = new JavaScriptSerializer();
      BaseJson = serializer.Deserialize<Dictionary<string, object>>(input.ReadToEnd());
    }

    public override string ToValue()
    {
      if (BaseJson.GetType() == null)
      {
        throw new InvalidOperationException("Load before this.");
      }

      if (BaseJson.GetType() == typeof(Dictionary<string, object>))
      {
        throw new InvalidCastException("This is not String");
      }

      return BaseJson.ToString();
    }

    protected override List<Tree> ToList()
    {
      if (BaseJson.GetType() == null)
      {
        throw new InvalidOperationException("Load before this.");
      }

      //if (!BaseJson.GetType())
      //{
      //  throw new InvalidCastException("Target is not List.");
      //}

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
        if (TreeJson.BaseJson[item].GetType() != typeof(Dictionary<string, object>))
        {
          TreeJson.BaseJson = TreeJson.BaseJson[item];
          break;
        }

        TreeJson.BaseJson = serializer.Deserialize<Dictionary<string, object>>(serializer.Serialize(TreeJson.BaseJson[item]));
      }

      return TreeJson;
    }

    protected override bool Exsits(List<string> paths)
    {
      var target = BaseJson;

      foreach (var item in paths)
      {
        if (target.ContainsKey(item))
        {
          target = (Dictionary<string,object>)target[item];
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
