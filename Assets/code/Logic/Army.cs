using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
//May be that should be extension

//public static class Ext
//{
//    public static uint Sum<Dictionary<PopType, uint>>(this IEnumerable<Dictionary<PopType, uint>> source, System.Func<PopType, uint> selector)
//    {
//        uint result;
//        foreach (KeyValuePair<TKey, TValue> next in source)
//            result += next.Value;
//        return result;
//    }
//}

public class Army
{

    Dictionary<PopType, uint> personal = new Dictionary<PopType, uint>();
    

    public void demobilize()
    {
    }
    //May be that should be extension
    public void recruitNew(PopType type, uint newMobilised)
    {
       // foreach (var unit in newMobilised)
            if (personal.ContainsKey(type))
                personal[type] += newMobilised;
            else
                personal.Add(type, newMobilised);
    }
    
    internal uint getSize()
    {
        uint result =0;
        foreach (var next in personal)
            result += next.Value;

        //return personal.Sum(x => x.Value);
        return result;
    }
    override public string ToString()
    {
        StringBuilder sb = new StringBuilder();
        
        foreach (var next in personal)
            sb.Append(next.Key.ToString()).Append(" ").Append(next.Value).Append(", ");
        sb.Append("Total size is ").Append(getSize());
        return sb.ToString();
    }
}
