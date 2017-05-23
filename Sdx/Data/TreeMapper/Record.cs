using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Sdx.Data.TreeMapper
{
    public class Record<T, R>
        where T : Sdx.Data.TreeMapper.Record.Item, new() 
        where R : Sdx.Db.Record
    {
        private Tree Tree;
        private Sdx.Db.RecordSet RecordSet;
        private Func<T, R, bool> Condition;

        public Record(Tree tree, Sdx.Db.RecordSet recordSet, Func<T, R, bool> condition)
        {
            Tree = tree;
            RecordSet = recordSet;
            Condition = condition;
        }

        public IEnumerable<Sdx.Data.TreeMapper.Record.Item> GetItems(string key)
        {
            foreach (var childTree in Tree.Get(key).List)
            {
                var treeItem = new T();
                treeItem.Tree = childTree;
                treeItem.Record = RecordSet.Select(rec => (R)rec).FirstOrDefault(rec => this.Condition(treeItem, rec));
                yield return treeItem;
            }
        }
    }
}
