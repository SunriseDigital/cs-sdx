using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Data.TreeMapper.Record
{
    public class Item
    {
        private Tree ChildTree;
        private Sdx.Db.Record TreeRecord;

        public Tree Tree
        {
            get
            {
                return ChildTree;
            }

            set
            {
                ChildTree = value;
            }
        }

        public Sdx.Db.Record Record
        {
            get
            {
                return TreeRecord;
            }

            set
            {
                TreeRecord =  value;
            }
        }
    }
}
