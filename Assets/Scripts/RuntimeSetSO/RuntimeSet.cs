using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RuntimeSet<T> : ScriptableObject
{
    private List<T> items = new List<T>();

    public List<T> Items { get => items; }
    public void Initialize()
    {
        items.Clear();
    }

    public T GetItemIndex(int index)
    {
        return items[index];
    }

    public void AddToList(T thingToAdd)
    {
        if (!items.Contains(thingToAdd))
        {
            items.Add(thingToAdd);
        }
    }

    public void RemoveFromList(T thingToRemove)
    {
        if (items.Contains(thingToRemove))
        {
            items.Remove(thingToRemove);
        }
    }
}
