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
      /// <summary>それ以外</summary>
      NONE,
      /// <summary>JPEG(jpg, jpeg, jpe, jfif)</summary>
      JPEG,
      /// <summary>GIF(gif)</summary>
      GIF,
      /// <summary>PNG(png)</summary>
      PNG,
      /// <summary>BMP(bmp, dib, rle)</summary>
      BMP,
      /// <summary>TIFF(tif, tiff)</summary>
      TIFF,
      /// <summary>EMF(emf)</summary>
      EMF,
      /// <summary>WMF(wmf) : 64</summary>
      WMF,
      /// <summary>ICON(ico) : 128</summary>
      ICON,
    }

    Stream stream;
    Bitmap bitmap = null;
    Sdx.Image.Format? type = null; //ファイルの種類

    public Image(Stream stream){
      this.stream = stream;
    }

    public Stream Stream
    {
      get
      {
        return this.stream;
      }
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
    /// 戻り値はSdx.Image.Format(enum型)。
    /// </summary>
    public Sdx.Image.Format Type
    {
      get {
        if(this.type == null){
          this.type = this.GetType();
        }

        return (Sdx.Image.Format)this.type;
      }
    }

    private Sdx.Image.Format GetType()
    {
      foreach (System.Drawing.Imaging.ImageCodecInfo ici in System.Drawing.Imaging.ImageCodecInfo.GetImageDecoders())
      {
        if (ici.FormatID == this.Bitmap.RawFormat.Guid)
        {
          //該当するFormatDescriptionがあったら、値をSdx.Image.Formatにキャストして返す。
          return (Sdx.Image.Format)(Enum.Parse(typeof(Sdx.Image.Format), ici.FormatDescription, false));
        }
      }

      return Sdx.Image.Format.NONE;
    }
  }
}
