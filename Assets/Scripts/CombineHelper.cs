using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombineHelper
{

    public Slot slot;
    public bool canMatch;
    public bool isSquare;
    public bool totalMatch;
    public List<Item> listedItem;

    public CombineHelper(Slot slot)
    {
        this.slot = slot;
        listedItem = new List<Item>();
    }

    public void Calculate()
    {
        // Try Vertical

        // Try Horizontal

        // Try Square
    }

    public void reset()
    {

    }
}
