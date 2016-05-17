using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Test.Orm
{
  public class ShopImage : Sdx.Db.Record
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    private string prevPath;

    static ShopImage()
    {
      Meta = Test.Orm.Table.ShopImage.Meta;
    }

    protected override void Init()
    {
      ValueWillUpdate["path"] = (prev, next, isRaw) => {
        if(prev != null)
        {
          prevPath = prev.ToString();
        }
      };
    }

    public void SetTempPath(string value)
    {
      this.SetValue("path", value + ".hoge");
    }

    public string GetImageWebPath()
    {
      var path = GetString("path");
      if (path == null || path.Length == 0)
      {
        return "";
      }
      return path.Substring(0, path.Length - 5);
    }

    public override void DisposeOnRollback()
    {
      var path = GetString("path");
      //画像が更新されていた。
      if (prevPath != path)
      {
        //pathの画像はゴミになるので削除しておく。
      }
    }

    protected override void RecordDidSave(Sdx.Db.Connection conn)
    {
      if(prevPath != null)
      {
        //差し替え画像なので削除
      }
    }

    protected override void RecordDidDelete(Sdx.Db.Connection conn)
    {
      if(HasValue("path"))
      {
        //レコード削除なので画像も消す。
      }
    }
  }
}
