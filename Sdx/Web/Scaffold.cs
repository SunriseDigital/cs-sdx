using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Web
{
  public class Scaffold
  {
    private const string CONTEXT_KEY = "SDX.WEB.SCAFFOLD.INSTANCES";
    public String Name { get; private set; }

    public static Scaffold Instance(string name)
    {
      return Context.Current.Vars.As<Dictionary<string, Scaffold>>(Scaffold.CONTEXT_KEY)[name];
    }

    public Scaffold(string name)
    {
      this.Name = name;
      Dictionary<string, Scaffold> instances = null;
      if (!Context.Current.Vars.ContainsKey(Scaffold.CONTEXT_KEY))
      {
        instances = new Dictionary<string, Scaffold>();
        Context.Current.Vars[Scaffold.CONTEXT_KEY] = instances;
      }
      else
      {
        instances = Context.Current.Vars.As<Dictionary<string, Scaffold>>(Scaffold.CONTEXT_KEY);
      }

      if (instances.ContainsKey(name))
      {
        throw new InvalidOperationException("Already exists `area` Scaffold");
      }

      instances[name] = this;
    }


    public Db.TableMeta Model { get; set; }

    public Db.Adapter Db { get; set; }

    public Db.RecordSet<Db.Record> List
    {
      get
      {
        var select = Db.CreateSelect();
        select.AddFrom(Model.CreateTable<Db.Table>());

        Db.RecordSet<Db.Record> records;
        using (var conn = Db.CreateConnection())
        {
          conn.Open();
          records = conn.FetchRecordSet<Db.Record>(select);
        }

        return records;
      }
    }
  }
}
