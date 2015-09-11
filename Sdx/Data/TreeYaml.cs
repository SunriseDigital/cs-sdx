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
    private List<Tree> listCache;
    private Dictionary<string, Tree> treeCache = new Dictionary<string, Tree>();

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
        if(this.listCache != null)
        {
          return this.listCache;
        }

        if (this.BaseNode == null)
        {
          throw new InvalidCastException("This is root node.");
        }

        if (!(this.BaseNode is YamlSequenceNode))
        {
          throw new InvalidCastException("Target is not List");
        }

        this.listCache = new List<Tree>();
        foreach (var node in ((YamlSequenceNode)this.BaseNode))
        {
          var row = new TreeYaml();
          row.BaseNode = node;
          this.listCache.Add(row);
        }

        return this.listCache;
      }
    }

    private const string FileExtension = "yml";

    internal override string DefaultFileExtension
    {
      get
      {
        return FileExtension;
      }
    }

    protected override Tree Get(string path, List<string> paths)
    {
      if(!this.treeCache.ContainsKey(path))
      {
        var tree = new TreeYaml();
        tree.BaseNode = this.FindTargetNode(paths);
        this.treeCache[path] = tree;
      }
      
      return this.treeCache[path];
    }

    public override void Load(StreamReader input)
    {
      var yaml = new YamlStream();
      yaml.Load(input);


      this.BaseNode = yaml.Documents[0].RootNode; ;
    }
  }
}
