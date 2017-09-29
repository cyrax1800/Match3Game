using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slot : MonoBehaviour
{
    public FieldController fieldController;
    public Item item;
    public int row, col;

    void Awake()
    {

    }

    // Use this for initialization
    void Start()
    {

    }

    public Item GenerateItem()
    {
        GameObject itemGO = Instantiate(fieldController.itemPrefab) as GameObject;
        itemGO.transform.SetParent(transform.parent);
        item = itemGO.GetComponent<Item>();
        item.slot = this;
        return item;
    }

    public Slot GetNeighBour(FieldController.Direction direction)
    {
        switch (direction)
        {
            case FieldController.Direction.UP:
                if (row > 0)
                    return fieldController.GetSlot(row - 1, col);
                break;
            case FieldController.Direction.DOWN:
                if (row > 0)
                    return fieldController.GetSlot(row + 1, col);
                break;
            case FieldController.Direction.LEFT:
                if (row > 0)
                    return fieldController.GetSlot(row, col - 1);
                break;
            case FieldController.Direction.RIGHT:
                if (row > 0)
                    return fieldController.GetSlot(row, col + 1);
                break;
        }

        return null;
    }

    public Slot[] GetAllNeighBour()
    {
        Slot[] slots = new Slot[4];
        slots[0] = GetNeighBour(FieldController.Direction.UP);
        slots[1] = GetNeighBour(FieldController.Direction.DOWN);
        slots[2] = GetNeighBour(FieldController.Direction.RIGHT);
        slots[3] = GetNeighBour(FieldController.Direction.LEFT);
        return slots;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
