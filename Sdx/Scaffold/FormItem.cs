using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Scaffold
{
  public class FormItem
  {
    private Param param;
    private Db.TableMeta tableMeta;

    public FormItem(Param param, Db.TableMeta tableMeta)
    {
      this.param = param;
      this.tableMeta = tableMeta;
    }

    public string Label
    {
      get
      {
        return this.param["label"];
      }
    }

    public Sdx.Html.FormElement CreateElement()
    {
      var methodName = "Create" + Sdx.Util.String.ToCamelCase(this.param["column"]) + "Element";
      var method = tableMeta.TableType.GetMethod(methodName);
      if (method!= null)
      {
        return (Sdx.Html.FormElement)method.Invoke(null, null);
      }
      else
      {
        var elem = new Sdx.Html.InputText();
        elem.Name = this.param["column"];
        return elem;
      }
    }
  }
}
