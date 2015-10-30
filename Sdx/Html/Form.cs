using System;
using System.Web;

namespace Sdx.Html
{
  public class Form : IHtml
  {
    public enum MethodType
    {
      Post,
      Get
    }

    private Tag tag = new Tag("form");

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

    public string Render(Attr attribute = null)
    {
      return this.tag.Render(attribute);
    }
  }
}