using System;
using System.Collections.Generic;
using Raven.Abstractions.Data;
using Raven.Json.Linq;

namespace Raven.Database.Storage
{
    public interface IListsStorageActions
    {
        void Set(string name, string key, RavenJObject data, UuidType uuidType);
        
        void Remove(string name, string key);

        IEnumerable<ListItem> Read(string name, Etag start, Etag end, int take);
        IEnumerable<ListItem> Read(string name, int start, int take);

        ListItem Read(string name, string key);

        ListItem ReadLast(string name);

        void RemoveAllBefore(string name, Etag etag);
        void RemoveAllOlderThan(string name, DateTime dateTime);
    }

    public class ListItem
    {
        public string Key;
        public Etag Etag;
        public RavenJObject Data;
        public DateTime CreatedAt;
    }
}
