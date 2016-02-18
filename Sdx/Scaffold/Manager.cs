using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;

namespace Sdx.Scaffold
{
  public class Manager
  {
    private const string CONTEXT_KEY = "SDX.SCAFFOLD.MANAGER.INSTANCES";
    private const string DEFAULT_NAME = "SDX.SCAFFOLD.MANAGER.DEFAULT_NAME";
    public String Name { get; private set; }

    public static void ClearContextCache()
    {
      Dictionary<string, Manager> instances = Context.Current.Vars.As<Dictionary<string, Manager>>(Manager.CONTEXT_KEY);
      instances.Clear();
    }

    public Manager(Db.TableMeta tableMeta, Sdx.Db.Adapter db, string name = Manager.DEFAULT_NAME)
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

      if (instances.ContainsKey(this.Name))
      {
        throw new InvalidOperationException("Already exists " + this.Name + " Manager");
      }

      this.TableMeta = tableMeta;

      instances[name] = this;

      Db = db;

      DisplayList = new ParamsList();
      FormList = new ParamsList();
    }

    internal Db.TableMeta TableMeta { get; set; }

    public Db.Adapter Db { get; private set; }

    private Db.Sql.Select CreateSelect()
    {
      var select = Db.CreateSelect();
      select.AddFrom(TableMeta.CreateTable());
      return select;
    }

    public string Title { get; set; }

    public ParamsList FormList { get; private set; }

    public ParamsList DisplayList { get; private set; }

    public Html.Form BuildForm()
    {
      var form = new Html.Form();

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
          //主キーはhidden
          if (TableMeta.Pkeys.Exists((column) => column == param["column"]))
          {
            elem = new Sdx.Html.InputHidden();
          }
          else
          {
            elem = new Sdx.Html.InputText();
          }
          
          elem.Name = param["column"];
        }

        if(!param.ContainsKey("label"))
        {
          throw new InvalidOperationException("Missing label param");
        }

        elem.Label = param["label"];

        form.SetElement(elem);
      }

      return form;
    }


    public Web.Url ListPageUrl { get; set; }
    public Web.Url EditPageUrl { get; set; }

    public static Manager CurrentInstance(string key)
    {
      if(key == null)
      {
        key = Manager.DEFAULT_NAME;
      }
      var instances = Context.Current.Vars.As<Dictionary<string, Manager>>(Manager.CONTEXT_KEY);
      return instances[key];
    }

    public Db.Record LoadRecord(NameValueCollection parameters, Sdx.Db.Connection conn)
    {
      var recordSet = FetchRecordSet(conn, (select) => {
        var exists = false;
        TableMeta.Pkeys.ForEach((column) =>
        {
          var values = parameters.GetValues(column);
          if (values != null && values.Length > 0 && values[0].Length > 0)
          {
            exists = true;
            select.Where.Add(column, values[0]);
          }
        });

        if (!exists)
        {
          return false;
        }

        return true;

      });

      Db.Record record;
      
      if(recordSet == null || recordSet.Count == 0)
      {
        record = TableMeta.CreateRecord();
        if (Group != null && Group.TargetValue != null)
        {
          record.SetValue(Group.TargetColumnName, Group.TargetValue);
        }
      }
      else
      {
        record = recordSet[0];
      }

      return record;
    }

    private Group.Base group;

    public Group.Base Group
    {
      get { return group; }
      set
      {
        group = value;
        group.Manager = this;
      }
    }

    private Db.RecordSet FetchRecordSet(Sdx.Db.Connection conn, Func<Db.Sql.Select, bool> filter)
    {
      var select = CreateSelect();
      var ret = filter(select);

      Db.RecordSet records = null;

      if(ret)
      {
        records = conn.FetchRecordSet(select);
      }

      return records;
    }

    public Db.RecordSet FetchRecordSet(Sdx.Db.Connection conn)
    {
      return FetchRecordSet(conn, (select) =>
      {
        if (Group != null)
        {
          if (Group.TargetValue != null)
          {
            select.Where.Add(Group.TargetColumnName, Group.TargetValue);
          }
        }

        return true;
      });
    }

    public void Save(Sdx.Db.Record record, NameValueCollection form, Sdx.Db.Connection conn)
    {
      var relationList = new ParamsList();
      var ownValues = new NameValueCollection();
      foreach (var param in FormList)
      {
        if(param.ContainsKey("relation"))
        {
          relationList.Add(param);
        }
        else
        {
          var columnName = param["column"];
          ownValues.Set(columnName, form[columnName]);
        }
      }

      record.Bind(ownValues);
      conn.Save(record);

      foreach (var param in relationList)
      {
        var rel = TableMeta.Relations[param["relation"]];
        foreach (var refId in form.GetValues(param["column"]))
        {
          var relRecord = rel.TableMeta.CreateRecord();
          relRecord.SetValue(rel.ReferenceKey, record.GetValue(rel.ForeignKey));
          relRecord.SetValue(param["column"], refId);
          conn.Save(relRecord);
        }
      }
    }
  }
}
