using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchHelper
{

    public Slot slot;
    public bool canMatch;
    public bool isSquare;
    public bool isCombine;
    public int totalMatch;
    public List<Item> listedVerticalItems;
    public List<Item> listedHorizontalItems;

    public Direction currentDirection;
    public int curRow, curCol;
    public Slot neighbour;

    public MatchHelper(Slot slot)
    {
        this.slot = slot;
        listedVerticalItems = new List<Item>();
        listedHorizontalItems = new List<Item>();
    }

    public IEnumerator DoMatch()
    {
        if (!canMatch) yield break;
        isCombine = false;
        if (listedVerticalItems.Count >= 2)
            totalMatch += listedVerticalItems.Count;
        if (listedHorizontalItems.Count >= 2)
            totalMatch += listedHorizontalItems.Count;

        isCombine = totalMatch >= 3;

        if (listedVerticalItems.Count >= 2)
        {
            for (int i = 0; i < listedVerticalItems.Count; i++)
            {
                listedVerticalItems[i].DestroyItem(slot, isCombine);
            }
        }
        if (listedHorizontalItems.Count >= 2)
        {
            for (int i = 0; i < listedHorizontalItems.Count; i++)
            {
                listedHorizontalItems[i].DestroyItem(slot, isCombine);
            }
        }
        if (slot.item != null)
            slot.item.DestroyItem(totalMatch);
    }

    public void Calculate()
    {
        //Check Vertical and Horizontal Swap
        CheckVerHorColor(Direction.UP);
        // Debug.Log("Vectical: " + listedVerticalItems.Count + " Horizontal: " + listedHorizontalItems.Count);
        if (listedVerticalItems.Count >= 2 || listedHorizontalItems.Count >= 2)
        {
            canMatch = true;
        }

        // Try Square
        if (!canMatch)
        {
            totalMatch = 0;
            listedVerticalItems.Clear();
            listedHorizontalItems.Clear();
            CheckSquareMatch();
        }
    }

    public void CheckVerHorColor(Direction direction)
    {
        if (direction != currentDirection)
        {
            curRow = slot.row;
            curCol = slot.col;
            neighbour = slot;
            currentDirection = direction;
        }
        if (currentDirection != Direction.NONE)
            neighbour = neighbour.GetNeighBour(currentDirection);

        if (currentDirection != Direction.NONE)
        {
            if (neighbour != null)
            {
                if (neighbour.item != null && slot.item != null)
                {
                    if (neighbour.item.color == slot.item.color)
                    {
                        if (currentDirection == Direction.UP || currentDirection == Direction.DOWN)
                        {
                            if (listedVerticalItems.IndexOf(neighbour.item) == -1)
                                listedVerticalItems.Add(neighbour.item);
                        }
                        else
                        {
                            if (listedHorizontalItems.IndexOf(neighbour.item) == -1)
                                listedHorizontalItems.Add(neighbour.item);
                        }
                        CheckVerHorColor(currentDirection);
                    }
                }
            }
        }
        if (currentDirection == Direction.UP)
            CheckVerHorColor(Direction.DOWN);
        else if (currentDirection == Direction.DOWN)
            CheckVerHorColor(Direction.LEFT);
        else if (currentDirection == Direction.LEFT)
            CheckVerHorColor(Direction.RIGHT);
    }

    public int[][] squareMatchChecker = new int[][] { new int[] { -1, -1 }, new int[] { -1, 1 }, new int[] { 1, -1 }, new int[] { 1, 1 } };
    public void CheckSquareMatch()
    {
        curRow = slot.row;
        curCol = slot.col;
        if (slot.item == null) return;
        for (int i = 0; i < squareMatchChecker.Length; i++)
        {
            listedVerticalItems.Clear();
            canMatch = false;
            Slot slot1 = slot.fieldController.GetSlot(curRow + squareMatchChecker[i][0], curCol + squareMatchChecker[i][1]);
            Slot slot2 = slot.fieldController.GetSlot(curRow, curCol + squareMatchChecker[i][1]);
            Slot slot3 = slot.fieldController.GetSlot(curRow + squareMatchChecker[i][0], curCol);
            if ((slot1 != null) && (slot2 != null) && (slot3 != null))
            {
                if ((slot1.item != null) && (slot2.item != null) && (slot3.item != null))
                {
                    if ((slot1.item.color == slot.item.color) && (slot2.item.color == slot.item.color) && (slot3.item.color == slot.item.color))
                    {
                        canMatch = true;
                        isSquare = true;
                        totalMatch = 4;
                        listedVerticalItems.Add(slot1.item);
                        listedVerticalItems.Add(slot2.item);
                        listedVerticalItems.Add(slot3.item);
                        break;
                    }
                }
            }
        }
    }

    public void reset()
    {
        totalMatch = 0;
        listedVerticalItems.Clear();
        listedHorizontalItems.Clear();
        currentDirection = Direction.NONE;
        canMatch = false;
    }
}
