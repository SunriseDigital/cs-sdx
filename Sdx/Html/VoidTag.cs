namespace Sdx.Html
{
  public class VoidTag : Tag
  {
    public VoidTag() : base() { }
    public VoidTag(string name) : base(name) { }

    public override string RenderEndTag()
    {
      return "";
    }

    protected override void init() { }
  }
}