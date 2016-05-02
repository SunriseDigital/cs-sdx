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
        throw new InvalidCastException("This is root node.");
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
      var Json = new Sdx.Data.TreeJson();
      var jobject = JObject.Parse(this.BaseJson.ToString());

      foreach (var item in paths)
      {
        Json.BaseJson = jobject.GetValue(item);
        if (Json.BaseJson.Type == JTokenType.Array)
        {
          this.BaseJson = JArray.Parse(Json.BaseJson.ToString());

        }       
      }
      
      return Json;
    }

    protected override bool Exsits(List<string> paths)
    {
      foreach (var item in paths)
      {
        var jobject = JObject.Parse(this.BaseJson.ToString());

        //if ()
        //{
        //  return false;
        //}
      }

      return true;
    }
  }
}
