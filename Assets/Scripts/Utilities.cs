using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class Utilities
{

    public static Direction GetDirectionGesture(Vector3 deltaPosition)
    {
        if (Math.Max(Math.Abs(deltaPosition.x), Math.Abs(deltaPosition.y)) == Math.Abs(deltaPosition.x))
        {
            if (deltaPosition.x < 0)
                return Direction.LEFT;
            else if (deltaPosition.x > 0)
                return Direction.RIGHT;
        }
        else
        {
            if (deltaPosition.y < 0)
                return Direction.DOWN;
            else if (deltaPosition.y > 0)
                return Direction.UP;
        }
        return Direction.NONE;
    }

    public static bool IsNeighbour(Slot currentSlot, Slot targetSlot)
    {
        if ((Math.Abs(currentSlot.col - targetSlot.col) == 1) && (currentSlot.row == targetSlot.row))
            return true;
        if ((Math.Abs(currentSlot.row - targetSlot.row) == 1) && (currentSlot.col == targetSlot.col))
            return true;
        return false;
    }

    public static int GetEnumEntries<T>() where T : struct, IConvertible
    {
        if (!typeof(T).IsEnum)
            throw new ArgumentException("T must be an enumerated type");

        return Enum.GetNames(typeof(T)).Length;
    }

    public static TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
    public static string ToTitleCase(this string str)
    {
        str = str.Replace("_", " ");
        str = str.ToLower();
        return textInfo.ToTitleCase(str);
    }
}
