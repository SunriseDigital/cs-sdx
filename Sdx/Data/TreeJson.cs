using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Sdx.Data
{
  public class TreeJson : Tree
  {
    private JToken BaseJson { get; set; }

    public override void Load(TextReader input)
    {
      this.BaseJson = JObject.Parse(input.ReadToEnd());
    }

    public override string ToValue()
    {
      if (this.BaseJson == null)
      {
        throw new InvalidCastException("Load before this.");
      }

      if (this.BaseJson.Type == JTokenType.Object)
      {
        throw new InvalidCastException("This is not String");
      }

      return this.BaseJson.ToString();
    }

    protected override List<Tree> ToList()
    {
      if (this.BaseJson == null)
      {
        throw new InvalidOperationException("Load before this.");
      }

      if (this.BaseJson.Type != JTokenType.Array)
      {
        throw new InvalidCastException("Target is not List.");
      }

      var list = new List<Tree>();
      foreach (var item in this.BaseJson)
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
      var target = JObject.Parse(this.BaseJson.ToString());

      foreach (var item in paths)
      {
        TreeJson.BaseJson = target.GetValue(item);
      }

      return TreeJson;
    }

    protected override bool Exsits(List<string> paths)
    {
      JObject jobject = JObject.Parse(this.BaseJson.ToString());

      var target = jobject.ToObject<Dictionary<string, object>>();

      foreach (var item in paths)
      {
        if (target.ContainsKey(item))
        {
          return true;
        }
      }

      return false;
    }
  }
}
