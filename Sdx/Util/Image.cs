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

    public static string GetFileExtension(ImageFormat format)
    {
      return ImageCodecInfo.GetImageEncoders().FirstOrDefault(x => x.FormatID == format.Guid).FilenameExtension;
    }
  }
}
