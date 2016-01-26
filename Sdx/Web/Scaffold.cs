using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Web
{
  public class Scaffold
  {
    public String Name { get; private set; }

    public static Scaffold Instance(string name)
    {
      return Context.Current.Vars.As<Dictionary<string, Scaffold>>("Sdx.Web.Scaffold.Instances")[name];
    }

    public Scaffold(string name)
    {
      this.Name = name;
      Dictionary<string, Scaffold> instances = null;
      if(!Context.Current.Vars.ContainsKey("Sdx.Web.Scaffold.Instances"))
      {
        instances = new Dictionary<string, Scaffold>();
        Context.Current.Vars["Sdx.Web.Scaffold.Instances"] = instances;
      }
      else
      {
        instances = Context.Current.Vars.As<Dictionary<string, Scaffold>>("Sdx.Web.Scaffold.Instances");
      }

      if(instances.ContainsKey(name))
      {
        throw new InvalidOperationException("Already exists `area` Scaffold");
      }

      instances[name] = this;
    }


    public Db.TableMeta Model { get; set; }

    public void RunList()
    {
      
    }
  }
}
