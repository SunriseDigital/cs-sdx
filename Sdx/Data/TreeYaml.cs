using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using YamlDotNet.RepresentationModel;

namespace Sdx.Data
{
  public class TreeYaml : Tree
  {
    private YamlNode BaseNode { get; set; }

    private YamlNode FindTargetNode(List<string> paths)
    {
      YamlNode target = this.BaseNode;

      foreach (var key in paths)
      {
        target = ((YamlMappingNode)target).Children[new YamlScalarNode(key)];
      }

      return target;
    }

    public override string ToValue()
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

    protected override List<Tree> ToList()
    {
      if (this.BaseNode == null)
      {
        throw new InvalidOperationException("Load before this.");
      }

      if (!(this.BaseNode is YamlSequenceNode))
      {
        throw new InvalidCastException("Target is not List.");
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

    protected override Tree BuildTree(List<string> paths)
    {
      var tree = new TreeYaml();
      tree.BaseNode = this.FindTargetNode(paths);
      return tree;
    }

    public override void Load(TextReader input)
    {
      var yaml = new YamlStream();
      yaml.Load(input);


      this.BaseNode = yaml.Documents[0].RootNode; ;
    }

    public override void Bind(string value)
    {
      //Bind自体が後付けで今現在TreeYamlでは使用していない為、例外を返しています。
      //用途が出てきたら、実装してください。
      throw new NotImplementedException();
    }

    public override string ToString()
    {
      return BaseNode.ToString();
    }

    protected override bool Exsits(List<string> paths)
    {
      YamlNode target = this.BaseNode;

      foreach (var key in paths)
      {
        var keyNode = new YamlScalarNode(key);
        if (((YamlMappingNode)target).Children.ContainsKey(keyNode))
        {
          target = ((YamlMappingNode)target).Children[new YamlScalarNode(key)];
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
