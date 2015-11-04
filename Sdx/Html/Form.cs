using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Sdx.Html
{
  public class Form
  {
    public enum MethodType
    {
      Post,
      Get
    }

    private Tag tag = new Tag("form");
    private Dictionary<string, Element> elements = new Dictionary<string, Element>();

    public Form()
    {
      this.Method = MethodType.Post;
    }

    public string Action
    {
      get
      {
        return this.tag.Attr["action"];
      }

      set
      {
        this.tag.Attr.Set("action", value);
      }
    }

    public void SetActionToCurrent()
    {
      if(HttpContext.Current == null)
      {
        throw new InvalidOperationException("Missing HttpContext.Current.");
      }

      this.Action = HttpContext.Current.Request.RawUrl;
    }

    public MethodType Method
    {
      get
      {
        if(this.tag.Attr["method"] == "post")
        {
          return MethodType.Post;
        }
        else
        {
          return MethodType.Get;
        }
      }

      set
      {
        if(value == MethodType.Post)
        {
          this.tag.Attr["method"] = "post";
        }
        else
        {
          this.tag.Attr["method"] = "get";
        }
      }
    }

    public Attr Attr
    {
      get
      {
        return this.tag.Attr;
      }
    }

    public Element this[string name]
    {
      get
      {
        return this.elements[name];
      }
    }

    public string Render(Attr attribute = null)
    {
      var builder = new StringBuilder();

      this.tag.RenderStartTag(builder, attribute);

      foreach(var kv in this.elements)
      {
        var elem = kv.Value;
        builder.Append(elem.Tag.Render());
      }

      builder.Append(this.tag.RenderEndTag());

      return builder.ToString();
    }

    public string RenderStartTag(Attr attribute = null)
    {
      return this.tag.RenderStartTag(attribute);
    }

    public string RenderEndTag()
    {
      return this.tag.RenderEndTag();
    }


    public void SetElement(Element element)
    {
      if(element.Name == null)
      {
        throw new InvalidOperationException("Element name is null.");
      }

      this.elements[element.Name] = element;
    }
  }
}