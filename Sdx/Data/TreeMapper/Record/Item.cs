using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Data.TreeMapper.Record
{
    public class Item
    {
        protected Dictionary<string, Sdx.Db.Record> treeRecords = null;
        protected Sdx.Data.Tree tree = null;

        public void AddTree(Sdx.Data.Tree tree)
        {
          this.tree = tree;
        }

        public void AddRecord(string key, Sdx.Db.Record record)
        {
          if (treeRecords == null)
          {
            treeRecords = new Dictionary<string, Db.Record> { };
          }

          treeRecords.Add(key, record);
        }
    }
}
