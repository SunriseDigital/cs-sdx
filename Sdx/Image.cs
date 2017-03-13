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

    private string Type
    {
      get { return this.type; }
      set { this.type = value; }
    }

    /// <summary>
    /// ファイルの形式(拡張子)を返す。
    /// </summary>
    /// <returns></returns>
    public string GetFileFormat()
    {
      if(this.Type != null){
        return this.Type;
      }

      try
      {
          foreach (System.Drawing.Imaging.ImageCodecInfo ici in System.Drawing.Imaging.ImageCodecInfo.GetImageDecoders())
          {
            if (ici.FormatID == this.Bitmap.RawFormat.Guid)
            {
              //該当するFormatDescriptionを返す。
              this.Type = ici.FormatDescription;
              return this.Type;
            }
          }

          return string.Empty;
      }
      catch
      {
          return string.Empty;
      }
    }

  }
}
