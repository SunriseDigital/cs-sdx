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
    public enum Format
    {
      /// <summary>それ以外 : 0</summary>
      NONE = 0,
      /// <summary>JPEG(jpg, jpeg, jpe, jfif) : 1</summary>
      JPEG = 1,
      /// <summary>GIF(gif) : 2</summary>
      GIF = 2,
      /// <summary>PNG(png) : 4</summary>
      PNG = 4,
      /// <summary>BMP(bmp, dib, rle) : 8</summary>
      BMP = 8,
      /// <summary>TIFF(tif, tiff) : 16</summary>
      TIFF = 16,
      /// <summary>EMF(emf) : 32</summary>
      EMF = 32,
      /// <summary>WMF(wmf) : 64</summary>
      WMF = 64,
      /// <summary>ICON(ico) : 128</summary>
      ICON = 128,
    }

    Stream stream;
    Bitmap bitmap = null;
    Sdx.Image.Format? type = null; //ファイルの種類

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
    public Sdx.Image.Format? Type
    {
      get {
        if(this.type == null){
          this.type = this.GetType();
        }

        return this.type;
      }
    }

    private Sdx.Image.Format GetType()
    {
      foreach (System.Drawing.Imaging.ImageCodecInfo ici in System.Drawing.Imaging.ImageCodecInfo.GetImageDecoders())
      {
        if (ici.FormatID == this.Bitmap.RawFormat.Guid)
        {
          //該当するFormatDescriptionを返す。
          return (Sdx.Image.Format)(Enum.Parse(typeof(Sdx.Image.Format), ici.FormatDescription, false));
        }
      }

      return Sdx.Image.Format.NONE;
    }

  }
}
