using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHeuristic
{
    Vector3 target { get; set; }


    float CalculateHeuristic(FlowFieldCell cell);
}

// Source - https://stackoverflow.com/a/21886340
// Posted by Knasterbax, modified by community. See post 'Timeline' for change history
// Retrieved 2026-08-08, License - CC BY-SA 4.0

/// <summary>
/// Comparer for comparing two keys, handling equality as beeing greater
/// Use this Comparer e.g. with SortedLists or SortedDictionaries, that don't allow duplicate keys
/// </summary>
/// <typeparam name="TKey"></typeparam>
public class DuplicateKeyComparer<TKey>
                :
             IComparer<TKey> where TKey : IComparable
{


    public int Compare(TKey x, TKey y)
    {
        int result = x.CompareTo(y);

        if (result == 0)
            return 1; // Handle equality as being greater. Note: this will break Remove(key) or
        else          // IndexOfKey(key) since the comparer never returns 0 to signal key equality
            return result;
    }

}
