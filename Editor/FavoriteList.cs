using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace SmartFavorites
{
    [Serializable]
    public class FavoriteList
    {
        public string name;
        public List<string> serializedIds;
        
        public FavoriteList(string name)
        {
            this.name = name;
            serializedIds = new List<string>();
        }

        public Object Get(int index)
        {
            if (serializedIds.Count < index)
                return null;
            
            return GlobalObjectId.TryParse(serializedIds[index], out GlobalObjectId obj) ?
                GlobalObjectId.GlobalObjectIdentifierToObjectSlow(obj) : null;
        }

        public bool Contains(GlobalObjectId objectId) =>
            serializedIds.Contains(objectId.ToString());

        public void Add(IEnumerable<GlobalObjectId> objectIds) =>
            serializedIds.AddRange(objectIds.Select(o => o.ToString()));

        public void Add(GlobalObjectId objectId) =>
            serializedIds.Add(objectId.ToString());

        public void Remove(List<GlobalObjectId> objectIds)
        {
            foreach (GlobalObjectId iObject in objectIds)
                serializedIds.Remove(iObject.ToString());
        }

        public void Remove(GlobalObjectId objectId) =>
            serializedIds.Remove(objectId.ToString());

        public void RemoveAt(int index) =>
            serializedIds.RemoveAt(index);

        public void Clear() =>
            serializedIds.Clear();
    }
}
