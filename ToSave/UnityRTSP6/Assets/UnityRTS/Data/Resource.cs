using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[Serializable]
public class Resource {

    public string Name;
    public int Amount;

}

[Serializable]
public class ResourceCollection {

    public List<Resource> Resources = new List<Resource>();

    public bool HasResources {
        get {
            for (int r = 0; r < Resources.Count; ++r) if (Resources[r].Amount > 0) return true;
            return false;
        }
    }

    public Resource GetResourceEntry(string name) {
        for (int r = 0; r < Resources.Count; ++r) {
            if (Resources[r].Name == name) return Resources[r];
        }
        return null;
    }
    public Resource RequireResource(string name) {
        var resource = GetResourceEntry(name);
        if (resource == null)
            Resources.Add(resource = new Resource() { Name = name, });
        return resource;
    }

    public void SetResource(string name, int amount) {
        RequireResource(name).Amount = amount;
    }
    public int GetResource(string name) {
        var resource = GetResourceEntry(name);
        return resource != null ? resource.Amount : 0;
    }
    public int TakeResources(string name, int amount) {
        var resource = GetResourceEntry(name);
        if (resource != null) {
            amount = Math.Min(resource.Amount, amount);
            if (resource.Amount >= amount) {
                resource.Amount -= amount;
                return amount;
            }
        }
        return 0;
    }
    public int DeliverResources(string name, int amount) {
        var resource = RequireResource(name);
        resource.Amount += amount;
        return amount;
    }

    public int Count { get { return Resources.Count; } }
    public Resource this[int id] {
        get { return Resources[id]; }
    }
    public int this[string name] {
        get { return GetResource(name); }
        set { SetResource(name, value); }
    }


    public void CloneFrom(ResourceCollection other) {
        int e = 0, o = 0;
        while (e < Resources.Count || o < other.Resources.Count) {
            int oid = o;
            if (e < Resources.Count) {
                for (; oid < other.Resources.Count; ++oid) if (other.Resources[oid].Name == Resources[e].Name) break;
            } else oid = other.Resources.Count;
            if (e == Resources.Count || oid < other.Resources.Count) {
                for (; o < oid; ++o) {
                    var otherEntity = other.Resources[o];
                    var myEntity = new Resource() { Name = otherEntity.Name };
                    myEntity.Amount = otherEntity.Amount;
                    Resources.Insert(e++, myEntity);
                }
            }
            if (e < Resources.Count) {
                if (oid < other.Resources.Count) {
                    if (oid != o) Debug.LogError("Id missmatch");
                    Resources[e++].Amount = other.Resources[o++].Amount;
                } else {
                    Resources.RemoveAt(e);
                }
            }
        }
    }
}