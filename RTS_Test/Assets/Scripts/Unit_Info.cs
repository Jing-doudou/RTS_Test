using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_Info
{
    public string path;
    protected string unitName;
    public Unit_Info(Unit_Ctrl unit)
    {
        string[] x = unit.name.Split("_");
        unitName = x[1];
        path = "UiHeadImage/" + unitName;
    }
  
}
