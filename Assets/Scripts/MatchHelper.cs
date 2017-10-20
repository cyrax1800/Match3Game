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

        ItemType itemType = ItemType.NONE;

        if (isSquare)
            itemType = ItemType.AIRPLANE;
        else if (listedVerticalItems.Count == 3 && listedHorizontalItems.Count == 0)
            itemType = ItemType.HORIZONTAL;
        else if (listedVerticalItems.Count == 0 && listedHorizontalItems.Count == 3)
            itemType = ItemType.VERTICAL;
        else if ((listedVerticalItems.Count == 4 && listedHorizontalItems.Count < 3) || (listedVerticalItems.Count < 3 && listedHorizontalItems.Count == 4))
            itemType = ItemType.COLOR_BOMB;
        else if (totalMatch == 4 && (listedVerticalItems.Count == 2 && listedHorizontalItems.Count == 2))
            itemType = ItemType.BOMB;
        else if (totalMatch == 5)
            itemType = ItemType.DYNAMITE;
        else if (totalMatch == 6)
            itemType = ItemType.BIOHAZARD;

        if (slot.item != null)
        {
            if (!slot.item.isNone()) itemType = slot.item.type;
            slot.item.DestroyItem(itemType);
        }
    }

    public void Calculate()
    {
        if (!slot.item) return;
        if (slot.item.isDropItem()) return;
        if (!slot.item.isNone() && !slot.item.isBoosterHasColor()) return;

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
                    if (isSameColorForMatchBetween(neighbour.item, slot.item))
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
                    if (isSameColorForMatchBetween(slot1.item, slot.item) &&
                        isSameColorForMatchBetween(slot2.item, slot.item) &&
                        isSameColorForMatchBetween(slot3.item, slot.item))
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

    public bool isSameColorForMatchBetween(Item item1, Item item2)
    {
        if (item1.isDropItem() || item2.isDropItem()) return false;
        if (!(item1.isNone() && !item1.isBoosterHasColor()) || (!item2.isNone() && !item2.isBoosterHasColor())) return false;
        return item1.color == item2.color;
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
