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
        private Dictionary<string, Sdx.Db.RecordSet> RecordSetDictionary;
        private string RecordKey;
        private Func<T, R, bool> Condition;

        public Record(Tree tree, Dictionary<string, Sdx.Db.RecordSet> recordSetList, Func<T, R, bool> condition)
        {
            Tree = tree;
            RecordSetDictionary = recordSetList;
            Condition = condition;
        }

        public IEnumerable<Sdx.Data.TreeMapper.Record.Item> GetItems()
        {
            foreach (var childTree in Tree.List)
            {
                yield return CreateItem(childTree);
            }
        }

        public IEnumerable<Sdx.Data.TreeMapper.Record.Item> GetItems(string key)
        {
            foreach (var childTree in Tree.Get(key).List)
            {
                yield return CreateItem(childTree);
            }
        }

        private T CreateItem(Tree childTree)
        {
            var treeItem = new T();
            treeItem.Tree = childTree;
            foreach (KeyValuePair<string, Sdx.Db.RecordSet> pair in RecordSetDictionary)
            {
              treeItem.AddRecord(pair.Key, pair.Value.Select(rec => (R)rec).FirstOrDefault(rec => this.Condition(treeItem, rec)));
            }

            return treeItem;
        }

        public int Count(string key = null)
        {
          if (key == null)
          {
            return Tree.Count;
          }
          else
          {
            return Tree.Get(key).Count;
          }
        }
    }
}
