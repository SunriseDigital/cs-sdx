using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace Sdx.Util
{
  public static class Image
  {
    /// <summary>
    /// 画像フォーマットを返す。判別不能ならNULLが返ります。
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    public static ImageCodecInfo GetImageCodecInfo(System.Drawing.Image image)
    {
      return ImageCodecInfo.GetImageDecoders().FirstOrDefault(codec => codec.FormatID == image.RawFormat.Guid);
    }

    public static ImageCodecInfo GetImageCodecInfo(System.Drawing.Imaging.ImageFormat format)
    {
      return ImageCodecInfo.GetImageDecoders().FirstOrDefault(codec => codec.FormatID == format.Guid);
    }

    /// <summary>
    /// 最大サイズを指定して必要であれば縮小した新しい<see cref="System.Drawing.Image"/>を生成して返す。
    /// 縮小の必要がないときはNULLが返ります。縮小が発生したかどうかを識別し適宜リソースの開放をしてください。
    /// </summary>
    /// <param name="image"></param>
    /// <param name="maxWidth"></param>
    /// <param name="maxHeight"></param>
    /// <returns></returns>
    public static System.Drawing.Image ScaleDown(System.Drawing.Image image, int? maxWidth = null, int? maxHeight = null)
    {
      double ratio;
      if(maxWidth == null && maxHeight == null)
      {
        return null;
      }
      else if(maxWidth == null)
      {
        ratio = (double)maxHeight / image.Height;
      }
      else if(maxHeight == null)
      {
        ratio = (double)maxWidth / image.Width;
      }
      else
      {
        var ratioX = (double)maxWidth / image.Width;
        var ratioY = (double)maxHeight / image.Height;
        ratio = Math.Min(ratioX, ratioY);
      }

      //既に縦横とも小さい
      if (ratio >= 1)
      {
        return null;
      }

      var newWidth = (int)(image.Width * ratio);
      var newHeight = (int)(image.Height * ratio);

      var newImage = new System.Drawing.Bitmap(newWidth, newHeight);
      using (var graphics = System.Drawing.Graphics.FromImage(newImage))
      {
        graphics.DrawImage(image, 0, 0, newWidth, newHeight);
      }

      return Image.Scale(image, ratio);
    }

    /// <summary>
    /// 最小サイズを指定して必要であれば拡大した新しい<see cref="System.Drawing.Image"/>を生成して返す。
    /// リサイズが発生したかどうかを識別し適宜リソースの開放をしてください。
    /// </summary>
    /// <param name="image"></param>
    /// <param name="minWidth"></param>
    /// <param name="minHeight"></param>
    /// <returns></returns>
    public static System.Drawing.Image ScaleUp(System.Drawing.Image image, int? minWidth = null, int? minHeight = null)
    {
      double ratio;
      if (minWidth == null && minHeight == null)
      {
        return null;
      }
      else if (minWidth == null)
      {
        ratio = (double)minHeight / image.Height;
      }
      else if (minHeight == null)
      {
        ratio = (double)minWidth / image.Width;
      }
      else
      {
        var ratioX = (double)minWidth / image.Width;
        var ratioY = (double)minHeight / image.Height;
        ratio = Math.Max(ratioX, ratioY);
      }

      //既に縦横とも大きい
      if (ratio <= 1)
      {
        return null;
      }

      return Image.Scale(image, ratio);
    }

    /// <summary>
    /// 比率を指定してリサイズした新しい<see cref="System.Drawing.Image"/>を生成して返す。
    /// `1`で等倍、`1`以下は縮小、`1`以上は拡大になります。このメソッドは確実に新しい<see cref="System.Drawing.Image"/>を生成します。
    /// </summary>
    /// <param name="image"></param>
    /// <param name="ratio"></param>
    /// <returns></returns>
    public static System.Drawing.Image Scale(System.Drawing.Image image, double ratio)
    {
      var newWidth = (int)(image.Width * ratio);
      var newHeight = (int)(image.Height * ratio);
      var newImage = new System.Drawing.Bitmap(newWidth, newHeight);
      using (var graphics = System.Drawing.Graphics.FromImage(newImage))
      {
        graphics.DrawImage(image, 0, 0, newWidth, newHeight);
      }

      return newImage;
    }

    /// <summary>
    /// <see cref="System.Drawing.Image.RawFormat"/>から拡張子を判別して返す。不明な場合は空文字を返します。
    /// <see cref="System.Drawing.Bitmap"/>などを利用しメモリから生成した<see cref="System.Drawing.Image"/>の`RawFormat`は空文字を返すので注意してください。
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    public static string GetFileExtension(System.Drawing.Image image)
    {
      var info = Image.GetImageCodecInfo(image);
      if(info == null)
      {
        return "";
      }

      return info.FilenameExtension.Split(';').First().Substring(2).ToLower();
    }

    /// <summary>
    /// <see cref="Sdx.Util.GetFileExtension(System.Drawing.Image image)"/>
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    public static string GetFileExtension(System.Drawing.Imaging.ImageFormat format)
    {
      var info = Image.GetImageCodecInfo(format);
      if (info == null)
      {
        return "";
      }

      return info.FilenameExtension.Split(';').First().Substring(2).ToLower();
    }
  }
}
