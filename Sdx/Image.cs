using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace Sdx
{
  public class Image
  {
    Stream stream;
    Bitmap bitmap = null;
    string type = null; //ファイルの種類

    public Image(Stream stream){
      this.stream = stream;
    }

    public Bitmap Bitmap
    {
      get{
        if(this.bitmap == null){
           this.bitmap = new Bitmap(this.stream);
        }

        return this.bitmap;
      }
    }

    public int Height
    {
      get{
        return this.Bitmap.Height;
      }
    }

    public int Width
    {
      get{
        return this.Bitmap.Width;
      }
    }

    /// <summary>
    /// 単位はbyte
    /// </summary>
    public long Size
    {
      get{
        return this.stream.Length;
      }
    }

    /// <summary>
    /// ファイルの形式(拡張子)を返す。
    /// </summary>
    private string Type
    {
      get {
        if(this.type == null){
          this.type = this.GetTypeString();
        }

        return this.type;
      }
    }

    private string GetTypeString()
    {
      foreach (System.Drawing.Imaging.ImageCodecInfo ici in System.Drawing.Imaging.ImageCodecInfo.GetImageDecoders())
      {
        if (ici.FormatID == this.Bitmap.RawFormat.Guid)
        {
          //該当するFormatDescriptionを返す。
          return ici.FormatDescription;
        }
      }

      return string.Empty;
    }

  }
}
