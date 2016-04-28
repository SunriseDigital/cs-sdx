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
    private string BaseJson { get; set; }

    public override void Load(TextReader input)
    {
      var json = JObject.Parse(input.ReadToEnd());
      this.BaseJson = Sdx.Util.Json.Encoder(json);
    }

    public override string ToValue()
    {
      if (this.BaseJson == null)
      {
        throw new InvalidCastException("This is root node.");
      }

      return this.BaseJson;
    }

    protected override List<Tree> ToList()
    {
      var list = new List<Tree>();
      return list;
    }

    protected override Tree BuildTree(List<string> paths)
    {
      Sdx.Data.TreeJson Json = new Sdx.Data.TreeJson();
      var jobject = JObject.Parse(this.BaseJson);
      
      foreach (var item in paths)
      {
        Json.BaseJson = jobject.GetValue(item).ToString();
      }

      return Json;
    }

    protected override bool Exsits(List<string> paths)
    {
      foreach(var item in paths){
        var jobject = JObject.Parse(this.BaseJson);
        
        //if ()
        //{
        //  return false;
        //}
      }

      return true;
    }
  }
}
