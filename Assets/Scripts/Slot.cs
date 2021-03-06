using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slot : MonoBehaviour
{
    public FieldController fieldController;
    public Item item;
    public int row, col;
    public MatchHelper matchHelper;

    void Awake()
    {
        matchHelper = new MatchHelper(this);
    }

    // Use this for initialization
    void Start()
    {

    }

    public Item GenerateItem(bool falling = true)
    {
        GameObject itemGO = Instantiate(fieldController.itemPrefab) as GameObject;
        itemGO.transform.SetParent(transform.parent);
        item = itemGO.GetComponent<Item>();
        item.slot = this;
        item.fieldController = fieldController;
        item.setPosition(falling);
        item.color = ItemColor.RANDOM;
        if (GameController.gameState == GameState.Initialize)
        {
            if (fieldController.test != null && fieldController.test.board.Count > 0)
            {
                int color = fieldController.test.board[row * fieldController.maxRow + col];
                if (color > 0)
                    item.color = (ItemColor)color - 1;
            }
        }

        if (falling) item.StartFalling();
        return item;
    }

    public void FallNext(bool force = false)
    {
        if (item != null)
        {
            Slot nextSlot = GetNeighBour(Direction.DOWN);
            if (nextSlot != null)
            {
                if (nextSlot.item == null)
                {
                    nextSlot.item = item;
                    item.slot = nextSlot;
                    item.StartFalling();
                    item = null;
                }
            }
        }
    }

    public bool CanFallNext()
    {
        Slot nextSlot = GetNeighBour(Direction.DOWN);
        if (nextSlot != null)
            if (nextSlot.item == null)
                return true;
        return false;
    }

    public Slot GetNeighBour(Direction direction)
    {
        switch (direction)
        {
            case Direction.UP:
                if (row > 0)
                    return fieldController.GetSlot(row - 1, col);
                break;
            case Direction.DOWN:
                if (row < fieldController.maxRow - 1)
                    return fieldController.GetSlot(row + 1, col);
                break;
            case Direction.LEFT:
                if (col > 0)
                    return fieldController.GetSlot(row, col - 1);
                break;
            case Direction.RIGHT:
                if (col < fieldController.maxCol - 1)
                    return fieldController.GetSlot(row, col + 1);
                break;
        }

        return null;
    }

    public Slot[] GetAllNeighBour()
    {
        Slot[] slots = new Slot[4];
        slots[0] = GetNeighBour(Direction.UP);
        slots[1] = GetNeighBour(Direction.DOWN);
        slots[2] = GetNeighBour(Direction.RIGHT);
        slots[3] = GetNeighBour(Direction.LEFT);
        return slots;
    }

    public MatchHelper TryMatch()
    {
        matchHelper.reset();
        matchHelper.Calculate();

        return matchHelper;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
