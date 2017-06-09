using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Data.TreeMapper.Record
{
    public class Item
    {
        public Tree Tree { get; set; }
        protected Dictionary<string, Sdx.Db.Record> TreeRecords = null;

        public void AddRecord(string key, Sdx.Db.Record record)
        {
          if (TreeRecords== null)
          {
            TreeRecords = new Dictionary<string,Db.Record>{};
          }
          
          TreeRecords.Add(key, record);
        }
    }
}
