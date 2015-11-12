using System.Web;

namespace Sdx.Html
{
  public class RawText : HtmlBase
  {
    public string Text { get; private set; }

    public RawText(string text):base(null)
    {
      this.Text = HttpUtility.HtmlEncode(text);
    }

    public override string Render(Attr attribute = null)
    {
      return this.Text;
    }

    public override string RenderStartTag(Attr attribute = null)
    {
      return null;
    }

    public override string RenderEndTag()
    {
      return null;
    }
  }
}