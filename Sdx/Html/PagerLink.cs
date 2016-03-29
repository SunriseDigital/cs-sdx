using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Sdx.Html
{
  public class PagerLink
  {
    public const string DEFAULT_VAR_NAME = "page";

    public PagerLink(Pager pager):this(pager, null)
    {
      BaseUrl = new Web.Url(HttpContext.Current.Request.Url.PathAndQuery);
    }

    public PagerLink(Pager pager, Web.Url baseUrl)
    {
      Pager = pager;
      BaseUrl = baseUrl;
      VarName = PagerLink.DEFAULT_VAR_NAME;
    }

    public Pager Pager { get; private set; }

    public Web.Url BaseUrl { get; set; }

    public string VarName { get; set; }

    private Tag CrateLinkTag(string linkText, string targetPage)
    {
      var url = (Web.Url)BaseUrl.Clone();
      Html.Tag tag;
      if (targetPage != null)
      {
        url.SetParam(VarName, targetPage);
        tag = new Tag("a");
        tag.Attr.Set("href", url.Build());
      }
      else
      {
        tag = new Tag("span");
        tag.Attr.AddClass("disabled");
      }

      tag.AddText(linkText);

      return tag;
    }

    public Tag GetPrev(string linkText)
    {
      return CrateLinkTag(linkText, Pager.HasPrev ? (Pager.Page - 1).ToString(): null);
    }

    public Tag GetNext(string linkText)
    {
      return CrateLinkTag(linkText, Pager.HasNext ? (Pager.Page + 1).ToString() : null);
    }
  }
}
