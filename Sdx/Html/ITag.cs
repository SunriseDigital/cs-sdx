namespace Sdx.Html
{
  public interface ITag
  {
    /// <summary>
    /// HTMLのタグとして正当な文字列を返す
    /// </summary>
    /// <param name="attribute">タグの属性</param>
    /// <returns></returns>
    string Render(Attr attribute = null);

    Attr Attr { get; }
  }
}