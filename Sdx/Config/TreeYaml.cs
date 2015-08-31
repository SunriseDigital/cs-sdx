﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using YamlDotNet.RepresentationModel;

namespace Sdx.Config
{
  public class TreeYaml : Tree
  {
    private static Dictionary<string, YamlNode> yamlData;
    static TreeYaml()
    {
      yamlData = new Dictionary<string, YamlNode>();
    }

    internal YamlNode BaseNode { get; set; }

    private YamlNode DetectTargetNode(List<string> paths)
    {
      YamlNode target;
      if (this.BaseNode == null)
      {
        var fileKey = paths[0];
        if (!yamlData.ContainsKey(fileKey))
        {
          yamlData[fileKey] = this.LoadYaml(fileKey);
        }

        target = yamlData[fileKey];
        paths.RemoveAt(0);
      }
      else
      {
        target = this.BaseNode;
      }

      foreach (var key in paths)
      {
        target = ((YamlMappingNode)target).Children[new YamlScalarNode(key)];
      }

      return target;
    }

    public override string Value
    {
      get
      {
        if (this.BaseNode == null)
        {
          throw new InvalidCastException("This is root node.");
        }

        if (!(this.BaseNode is YamlScalarNode))
        {
          throw new InvalidCastException("Target is not string.");

        }

        return ((YamlScalarNode)this.BaseNode).Value;
      }
    }

    public override List<Tree> List
    {
      get
      {
        if (this.BaseNode == null)
        {
          throw new InvalidCastException("This is root node.");
        }

        if (!(this.BaseNode is YamlSequenceNode))
        {
          throw new InvalidCastException("Target is not List");
        }

        var list = new List<Tree>();
        foreach (var node in ((YamlSequenceNode)this.BaseNode))
        {
          var row = new TreeYaml();
          row.BaseNode = node;
          list.Add(row);
        }

        return list;
      }
    }

    protected override Tree Get(List<string> paths)
    {
      var tree = new TreeYaml();
      tree.BaseNode = this.DetectTargetNode(paths);
      return tree;
    }

    private YamlNode LoadYaml(string fileKey)
    {
      var path = this.BaseDir + Path.DirectorySeparatorChar + fileKey + ".yml";
      var fs = new FileStream(path, FileMode.Open);
      var input = new StreamReader(fs, Encoding.GetEncoding("utf-8"));


      var yaml = new YamlStream();
      yaml.Load(input);

      return yaml.Documents[0].RootNode;
    }
  }
}
