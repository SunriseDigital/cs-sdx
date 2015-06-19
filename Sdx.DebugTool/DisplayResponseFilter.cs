using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sdx.DebugTool
{
  class DisplayResponseFilter : Stream
  {
    private Stream stream;
    private StreamWriter streamWriter;
    private String html = "";

    public DisplayResponseFilter(Stream stm)
    {
      stream = stm;
      streamWriter = new StreamWriter(stream, System.Text.Encoding.UTF8);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      MemoryStream ms = new MemoryStream(buffer, offset, count, false);
      StreamReader sr = new StreamReader(ms, System.Text.Encoding.UTF8);
      String s;
      while ((s = sr.ReadLine()) != null)
      {
        html += s;
      }

      if (html.Contains("</body>"))
      {
        var debugString = "<div class=\"sdx-debug-wrapper\">";
        Int64 prevElapsed = -1;
        foreach (Dictionary<String, Object> dic in Debug.Logs)
        {
          var totalElapsed = (Int64)dic["elapsedMsec"];
          Int64 currentElapsed = 0;
          if (prevElapsed != -1)
          {
            currentElapsed = totalElapsed - prevElapsed;
          }
          
          debugString += WrapTag(
            Debug.Dump(dic["value"]),
            dic["title"] as String,
            totalElapsed,
            currentElapsed
          );

          prevElapsed = totalElapsed;
        }
        debugString += "</div>";
        html = html.Replace("</body>", debugString + "</body>");
        streamWriter.Write(html);
        streamWriter.Flush();
      }
    }

    public String WrapTag(String value, String title, Int64 totalElapsed, Int64 currentElapsed)
    {
      DateTime now = DateTime.Now;
      return
        "<div class=\"sdx-debug-item\">" +
          "<div class=\"sdx-debug-title\">"+
            "[" + currentElapsed / 1000.0 + "/" + totalElapsed / 1000.0 + "]" + title + 
          "</div>"+
          "<pre class=\"sdx-debug-value\">" + value + "</pre>" +
        "</div>";
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      throw new NotSupportedException();
    }

    public override bool CanRead
    {
      get { return false; }
    }

    public override bool CanSeek
    {
      get { return false; }
    }

    public override bool CanWrite
    {
      get { return true; }
    }

    public override long Length
    {
      get { throw new NotSupportedException(); }
    }

    public override long Position
    {
      get { throw new NotSupportedException(); }
      set { throw new NotSupportedException(); }
    }

    public override void Flush()
    {
      stream.Flush();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
      throw new NotSupportedException();
    }
  }
}
