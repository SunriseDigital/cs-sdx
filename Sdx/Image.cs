using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Sdx
{
  public class Image
  {
    Stream stream;
    string height;
    string width;
    string size; //単位はbyte
    string type; //ファイルの種類

    public Image(Stream stream){
      this.stream = stream;
    }

    public string Height
    {
      get{return this.height;}
      set{this.height = value;}
    }

    public string Width
    {
      get{return this.width;}
      set{this.width = value;}
    }

    public string Size
    {
      get{return this.size;}
      set{this.size = value;}
    }

    public string Type
    {
      get{return this.type;}
      set{this.type = value;}
    }
  }
}
