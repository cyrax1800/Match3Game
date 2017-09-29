using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{

    public FieldController fieldController;
    public Slot slot;
    public SpriteRenderer spriteRenderer;
    public Utilities.Color color;

    public bool isSwapping;

    Slot neighbourSlot;
    Item neighbourItem;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Use this for initialization
    void Start()
    {
        transform.position = slot.transform.position;
        GenColor();
    }

    void GenColor(Utilities.Color targetColor = Utilities.Color.RANDOM)
    {

        int row = slot.row;
        int col = slot.col;

        List<int> remainColor = new List<int>();

        bool isSameColor = false;

        for (int i = 0; i < fieldController.gameController.colorVariant; i++)
        {
            // Check with left Slot
            isSameColor = false;
            if (col > 1)
            {
                Slot neighbourSlot = fieldController.GetSlot(row, col - 1);
                if (neighbourSlot != null && neighbourSlot.item != null)
                {
                    if ((int)neighbourSlot.item.color == i)
                        isSameColor = true;
                }
            }

            // Check with Right Slot
            if (col < fieldController.gameController.colorVariant - 1)
            {
                Slot neighbourSlot = fieldController.GetSlot(row, col + 1);
                if (neighbourSlot != null && neighbourSlot.item != null)
                {
                    if ((int)neighbourSlot.item.color == i)
                        isSameColor = true;
                }
            }

            // Check with Top Slot
            if (row > -1)
            {
                Slot neighbourSlot = fieldController.GetSlot(row - 1, col);
                if (neighbourSlot != null && neighbourSlot.item != null)
                {
                    if ((int)neighbourSlot.item.color == i)
                        isSameColor = true;
                }
            }

            if (!isSameColor)
                remainColor.Add(i);
        }

        if (targetColor == Utilities.Color.RANDOM)
        {
            targetColor = (Utilities.Color)remainColor[UnityEngine.Random.Range(0, remainColor.Count)];
        }
        spriteRenderer.sprite = fieldController.GetSpriteOfColor(targetColor);

        this.color = targetColor;
    }

    public void Swap(Slot target)
    {
        neighbourSlot = target;
        neighbourItem = target.item;

        StartCoroutine(Swapping());
    }

    IEnumerator Swapping()
    {
        yield return null;
        isSwapping = true;

        bool backMove = false;

        neighbourSlot.item = this;
        slot.item = neighbourItem;

        CombineHelper combineHelperThis = neighbourSlot.TryMatch();

        float startTime = Time.time;
        float duration = .25f;
        float t = 0;
        Vector2 startPosition = slot.transform.position;
        Vector2 targetPosition = neighbourSlot.transform.position;

        while (t < 1)
        {
            t = (Time.time - startTime) / duration;
            transform.position = Vector2.Lerp(startPosition, targetPosition, t);
            neighbourItem.transform.position = Vector2.Lerp(targetPosition, startPosition, t);
            yield return null;
        }

        backMove = true;

        if (backMove)
        {
            neighbourSlot.item = neighbourItem;
            slot.item = this;
            startTime = Time.time;
            t = 0;
            while (t < 1)
            {
                t = (Time.time - startTime) / duration;
                transform.position = Vector2.Lerp(targetPosition, startPosition, t);
                neighbourItem.transform.position = Vector2.Lerp(startPosition, targetPosition, t);
                yield return null;
            }
        }

        isSwapping = false;

    }

    // Update is called once per frame
    void Update()
    {

    }
}
