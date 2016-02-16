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

      DisplayList = new ParamList();
      FormList = new ParamList();
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

    public ParamList FormList { get; private set; }

    public ParamList DisplayList { get; private set; }

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

    public Db.Record LoadRecord(NameValueCollection parameters)
    {
      var recordSet = FetchRecordSet((select) => {
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

    private Db.RecordSet FetchRecordSet(Func<Db.Sql.Select, bool> filter)
    {
      var select = CreateSelect();
      var ret = filter(select);

      Db.RecordSet records = null;

      if(ret)
      {
        using (var conn = Db.CreateConnection())
        {
          conn.Open();
          records = conn.FetchRecordSet(select);
        }
      }

      return records;
    }

    private bool initializedGroup  = false;
    public Html.Select InitGroup()
    {
      if(this.Group == null)
      {
        return null;
      }

      if (initializedGroup)
      {
        return null;
      }

      initializedGroup = true;


      Group.TargetValue = HttpContext.Current.Request.QueryString[Group.TargetColumnName];
      Html.Select selector = null ;
      if(Group.HasSelector)
      {
        selector = Group.BuildSelector();
      }
      
      if(Group.TargetValue != null)
      {
        ListPageUrl.AddParam(Group.TargetColumnName, Group.TargetValue);
        EditPageUrl.AddParam(Group.TargetColumnName, Group.TargetValue);
      }
      else if(Group.Strict)
      {
        if(Group.DefaultValue != null || selector != null)
        {
          string value = null;
          if(Group.DefaultValue != null)
          {
            value = Group.DefaultValue;
          }
          else if (selector != null)
          {
            value = selector.Options.First().Tag.Attr["value"];
          }

          ListPageUrl.AddParam(Group.TargetColumnName, value);
          HttpContext.Current.Response.Redirect(ListPageUrl.Build(), true);
        }


        if(Sdx.Context.Current.HttpErrorHandler.HasHandler(404))
        {
          Sdx.Context.Current.HttpErrorHandler.Invoke(404);
        }
        else
        {
          throw  new HttpException(404, "Missing " + Group.TargetColumnName + " parameter");
        }
      }

      return selector;
    }

    public Db.RecordSet FetchRecordSet()
    {
      return FetchRecordSet((select) =>
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
  }
}
