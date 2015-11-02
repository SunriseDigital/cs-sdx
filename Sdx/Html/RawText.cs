namespace Sdx.Html
{
  public class RawText : IHtml
  {
    public string Text { get; private set; }

    public RawText(string text)
    {
      this.Text = text;
    }

    public string Render(Attr attribute = null)
    {
      return this.Text;
    }
  }
}