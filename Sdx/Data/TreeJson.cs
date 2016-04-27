using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sdx.Data
{
  public class TreeJson : Tree
  {
    string BaseJson { get; set; }

    public override void Load(TextReader input)
    {
      var json = JObject.Parse(input.ReadToEnd());
    }

    public override string ToValue()
    {
      throw new NotImplementedException();
    }

    protected override List<Tree> ToList()
    {
      throw new NotImplementedException();
    }

    protected override Tree BuildTree(List<string> paths)
    {
      Sdx.Data.TreeJson Json = new Sdx.Data.TreeJson();

      //Json.BaseJson = Sdx.Util.Json.Encoder();
      return Json;
    }

    protected override bool Exsits(List<string> paths)
    {
      throw new NotImplementedException();
    }
  }
}
