using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Culture
{
    public string name;
    List<Culture> allCultures = new List<Culture>();
    public Culture(string iname)
    {
        name = iname;
        allCultures.Add(this);

    }
}
