using System;
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

    protected override string GetString(List<string> paths)
    {
      var target = this.DetectTargetNode(paths);

      return this.BuildStringFrom(target);
    }

    private string BuildStringFrom(YamlNode target)
    {
      if (!(target is YamlScalarNode))
      {
        throw new InvalidCastException("Target is not string.");

      }

      return ((YamlScalarNode) target).Value;
    }

    protected override Dictionary<string, string> GetStrDic(List<string> paths)
    {
      var target = this.DetectTargetNode(paths);

      if (!(target is YamlMappingNode))
      {
        throw new InvalidCastException("Target is not Dictionary.");
      }

      var dic = new Dictionary<string, string>();
      foreach (var entry in ((YamlMappingNode)target).Children)
      {
        if (!(entry.Value is YamlScalarNode))
        {
          throw new InvalidCastException(entry.Key + " value is not string.");
        }

        dic[entry.Key.ToString()] = entry.Value.ToString();
      }

      return dic;
    }

    protected override List<Tree> GetTreeList(List<string> paths)
    {
      var target = this.DetectTargetNode(paths);

      return this.BuildListTreeFrom(target);
    }

    private List<Tree> BuildListTreeFrom(YamlNode target)
    {
      if (!(target is YamlSequenceNode))
      {
        throw new InvalidCastException("Target is not List");
      }

      var list = new List<Tree>();
      foreach (var node in ((YamlSequenceNode)target))
      {
        var row = new TreeYaml();
        row.BaseNode = node;
        list.Add(row);
      }

      return list;
    }

    protected override List<string> GetStrList(List<string> paths)
    {
      var target = this.DetectTargetNode(paths);

      return this.BuildListFrom(target);
    }

    private List<string> BuildListFrom(YamlNode target)
    {
      if (!(target is YamlSequenceNode))
      {
        throw new InvalidCastException("Target is not List");
      }

      var list = new List<string>();
      foreach (YamlScalarNode node in ((YamlSequenceNode)target))
      {
        list.Add(node.ToString());
      }

      return list;
    }

    protected override Dictionary<string, Tree> GetTreeDic(List<string> paths)
    {
      var target = this.DetectTargetNode(paths);

      if (!(target is YamlMappingNode))
      {
        throw new InvalidCastException("Target is not Dictionary.");
      }

      var dic = new Dictionary<string, Tree>();
      foreach (var entry in ((YamlMappingNode)target).Children)
      {
        var row = new TreeYaml();
        row.BaseNode = entry.Value;

        dic[entry.Key.ToString()] = row;
      }

      return dic;
    }

    public override string StringValue
    {
      get
      {
        if (this.BaseNode == null)
        {
          throw new InvalidCastException("This is root node.");
        }

        return this.BuildStringFrom(this.BaseNode);
      }
    }

    public override List<string> StrListValue
    {
      get
      {
        if (this.BaseNode == null)
        {
          throw new InvalidCastException("This is root node.");
        }

        return this.BuildListFrom(this.BaseNode);
      }
    }

    public override List<Tree> TreeListValue
    {
      get
      {
        if (this.BaseNode == null)
        {
          throw new InvalidCastException("This is root node.");
        }

        return this.BuildListTreeFrom(this.BaseNode);
      }
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
