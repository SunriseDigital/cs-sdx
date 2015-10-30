namespace Sdx.Html
{
  public class RawText : IHtml
  {
    private string text;

    public RawText(string text)
    {
      this.text = text;
    }

    public string Render(Attr attribute = null)
    {
      return this.text;
    }
  }
}