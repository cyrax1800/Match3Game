using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{

    public enum Color
    {
        RANDOM = -1,
        RED,
        BLUE,
        GREEN,
        YELLOW,
        PURPLE,
        wHITE
    }

    public static FieldController.Direction GetDirectionGesture(Vector3 deltaPosition)
    {
        if (Math.Max(Math.Abs(deltaPosition.x), Math.Abs(deltaPosition.y)) == Math.Abs(deltaPosition.x))
        {
            if (deltaPosition.x < 0)
                return FieldController.Direction.LEFT;
            else if (deltaPosition.x > 0)
                return FieldController.Direction.RIGHT;
        }
        else
        {
            if (deltaPosition.y < 0)
                return FieldController.Direction.DOWN;
            else if (deltaPosition.y > 0)
                return FieldController.Direction.UP;
        }
        return FieldController.Direction.NONE;
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
}
