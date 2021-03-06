﻿using System;
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
    private dynamic baseJson;
    private dynamic BaseJson { get { return baseJson; } set { initialize = true; baseJson = value; } }
    private bool initialize = false;

    public override void Load(TextReader input)
    {
      Bind(input.ReadToEnd());
    }

    public override void Bind(string value)
    {
      var serializer = new JavaScriptSerializer();
      if (value == "[]")
      {
        value = "{}";
      }

      BaseJson = serializer.Deserialize<Dictionary<string, object>>(value);
    }

    public override string ToValue()
    {
      if (initialize == false)
      {
        throw new InvalidOperationException("Load before this.");
      }

      if (BaseJson == null)
      {
        return null;
      }

      if (BaseJson is Dictionary<string, object>)
      {
        throw new InvalidCastException("This is not String");
      }

      return BaseJson.ToString();
    }

    protected override List<Tree> ToList()
    {
      if (initialize == false)
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

        if(target == null){
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
