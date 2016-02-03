using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Scaffold
{
  public class Manager
  {
    private const string CONTEXT_KEY = "SDX.SCAFFOLD.MANAGER.INSTANCES";
    public String Name { get; private set; }

    public Manager(Db.TableMeta tableMeta, Sdx.Db.Adapter db, string name)
    {
      this.Name = name;
      Dictionary<string, Manager> instances = null;
      if (!Context.Current.Vars.ContainsKey(Manager.CONTEXT_KEY))
      {
        instances = new Dictionary<string, Manager>();
        Context.Current.Vars[Manager.CONTEXT_KEY] = instances;
      }
      else
      {
        instances = Context.Current.Vars.As<Dictionary<string, Manager>>(Manager.CONTEXT_KEY);
      }

      if (instances.ContainsKey(name))
      {
        throw new InvalidOperationException("Already exists `area` Manager");
      }

      this.TableMeta = tableMeta;

      instances[name] = this;

      Db = db;

      DisplayList = new ParamList();
      FormList = new ParamList();
    }


    private Db.TableMeta TableMeta { get; set; }

    private Db.Adapter Db { get; set; }

    public dynamic RecordSet
    {
      get
      {
        var select = Db.CreateSelect();
        select.AddFrom(TableMeta.CreateTable<Db.Table>());

        dynamic records;
        using (var conn = Db.CreateConnection())
        {
          conn.Open();
          var method = conn.GetType().GetMethod("FetchRecordSet").MakeGenericMethod(TableMeta.RecordType);
          records = method.Invoke(conn, new object[] { select });
        }

        return records;
      }
    }

    public string Title { get; set; }

    public ParamList FormList { get; private set; }

    public ParamList DisplayList { get; private set; }

    public Html.Form BuildForm()
    {
      Form = new Html.Form();

      foreach (var param in FormList)
      {
        Html.FormElement elem;
        var methodName = "Create" + Sdx.Util.String.ToCamelCase(param["column"]) + "Element";
        var method = TableMeta.TableType.GetMethod(methodName);
        if (method != null)
        {
          elem = (Sdx.Html.FormElement)method.Invoke(null, null);
        }
        else
        {
          elem = new Sdx.Html.InputText();
          elem.Name = param["column"];
        }

        elem.Label = param["label"];

        Form.SetElement(elem);
      }

      return Form;
    }

    public Web.Url ReturnUrl { get; set; }

    public Html.Form Form { get; private set; }

    public static Manager CurrentInstance(string key)
    {
      var instances = Context.Current.Vars.As<Dictionary<string, Manager>>(Manager.CONTEXT_KEY);
      return instances[key];
    }
  }
}
