using System.Web;

namespace Sdx.Html
{
  public class RawText : HtmlBase
  {
    public string Text { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    /// <param name="htmlEncode">HTMLエンティティにエンコードするかどうか。デフォルトtrue</param>
    public RawText(string text, bool htmlEncode = true):base(null)
    {
      this.Text = htmlEncode ? HttpUtility.HtmlEncode(text) : text;
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