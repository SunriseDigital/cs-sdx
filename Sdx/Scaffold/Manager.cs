using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Scaffold
{
  public class Manager<T> where T : Sdx.Db.Record, new()
  {
    public const string CONTEXT_KEY = "SDX.SCAFFOLD.MANAGER.INSTANCES";
    public String Name { get; private set; }

    public Manager(Sdx.Db.Adapter db, string name)
    {
      this.Name = name;
      Dictionary<string, Manager<T>> instances = null;
      if (!Context.Current.Vars.ContainsKey(Manager<T>.CONTEXT_KEY))
      {
        instances = new Dictionary<string, Manager<T>>();
        Context.Current.Vars[Manager<T>.CONTEXT_KEY] = instances;
      }
      else
      {
        instances = Context.Current.Vars.As<Dictionary<string, Manager<T>>>(Manager<T>.CONTEXT_KEY);
      }

      if (instances.ContainsKey(name))
      {
        throw new InvalidOperationException("Already exists `area` Manager");
      }

      var prop = typeof(T).GetProperty("Meta");
      if (prop == null)
      {
        throw new NotImplementedException("Missing Meta property in " + typeof(T));
      }

      this.TableMeta = prop.GetValue(null, null) as Db.TableMeta;
      if (this.TableMeta == null)
      {
        throw new NotImplementedException("Initialize TableMeta for " + typeof(T));
      }

      instances[name] = this;

      Db = db;

      DisplayList = new ParamList();
      FormList = new ParamList();
    }


    private Db.TableMeta TableMeta { get; set; }

    private Db.Adapter Db { get; set; }

    public Db.RecordSet<T> RecordSet
    {
      get
      {
        var select = Db.CreateSelect();
        select.AddFrom(TableMeta.CreateTable<Db.Table>());

        Db.RecordSet<T> records;
        using (var conn = Db.CreateConnection())
        {
          conn.Open();
          records = conn.FetchRecordSet<T>(select);
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
  }
}
