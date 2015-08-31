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

    protected override object GetData<T>(List<string> paths)
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

      foreach(var key in paths)
      {
        target = ((YamlMappingNode)target).Children[new YamlScalarNode(key)];
      }

      var targetType = typeof(T);
      if (targetType == typeof(string))
      {
        if(target is YamlScalarNode)
        {
          return target.ToString();
        }

        throw new InvalidCastException("Target is not string.");
      }
      else if (targetType == typeof(DateTime))
      {
        if (target is YamlScalarNode)
        {
          return Convert.ToDateTime(target.ToString());
        }

        throw new InvalidCastException("Target is not DateTime.");
      }
      else if (targetType == typeof(Dictionary<string, string>))
      {
        if (target is YamlMappingNode)
        {
          var dic = new Dictionary<string, string>();
          foreach(var entry in ((YamlMappingNode) target).Children)
          {
            if(!(entry.Value is YamlScalarNode))
            {
              throw new InvalidCastException(entry.Key + " value is not string.");
            }

            dic[entry.Key.ToString()] = entry.Value.ToString();
          }

          return dic;
        }

        throw new InvalidCastException("Target is not Dictionary.");
      }
      else if (targetType == typeof(List<Tree>))
      {
        if(target is YamlSequenceNode)
        {
          var list = new List<Tree>();
          foreach(var node in ((YamlSequenceNode)target))
          {
            var row = new TreeYaml();
            row.BaseNode = node;
            list.Add(row);
          }

          return list;
        }

        throw new InvalidCastException("Target is not List");
      }
      else if(targetType == typeof(List<string>))
      {
        if (target is YamlSequenceNode)
        {
          var list = new List<string>();
          foreach (YamlScalarNode node in ((YamlSequenceNode)target))
          {
            list.Add(node.ToString());
          }

          return list;
        }

        throw new InvalidCastException("Target is not List");
      }

      return target;
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
