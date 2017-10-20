using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public FieldController fieldController;
    public Slot slot;
    public SpriteRenderer spriteRenderer;
    public ItemColor color;
    public ItemType type = ItemType.NONE;

    public bool isSwapping;
    public bool isFalling;
    public bool isBooster;
    public bool justCreatedBooster;

    Slot neighbourSlot;
    Item neighbourItem;

    Item nextTarget;
    ItemColor targetColor = ItemColor.RANDOM;

    void Awake()
    {
        isBooster = false;
        justCreatedBooster = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Use this for initialization
    void Start()
    {
        GenColor(color);
    }

    public void setPosition(bool falling = true)
    {
        transform.position = slot.transform.position;
        if (falling)
        {
            transform.position += fieldController.GetTopPosition();
        }
    }

    void GenColor(ItemColor targetColor = ItemColor.RANDOM)
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

        if (targetColor == ItemColor.RANDOM)
        {
            targetColor = (ItemColor)remainColor[UnityEngine.Random.Range(0, remainColor.Count)];
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
        if (neighbourSlot.item) neighbourSlot.item.isSwapping = true;

        bool backMove = false;

        neighbourSlot.item = this;
        slot.item = neighbourItem;

        MatchHelper matchHelperThis = neighbourSlot.TryMatch();
        MatchHelper matchHelperNeighbour = slot.TryMatch();

        float startTime = Time.time;
        float duration = .25f;
        float t = 0;
        Vector2 startPosition = slot.transform.position;
        Vector2 targetPosition = neighbourSlot.transform.position;

        while (t < 1)
        {
            t = (Time.time - startTime) / duration;
            transform.position = Vector2.Lerp(startPosition, targetPosition, t);
            if (neighbourItem != null)
                neighbourItem.transform.position = Vector2.Lerp(targetPosition, startPosition, t);
            yield return null;
        }

        if (matchHelperThis.canMatch || matchHelperNeighbour.canMatch || isBooster || neighbourItem.isBooster)
        {
            if (neighbourItem != null)
                neighbourItem.slot = slot;
            slot = neighbourSlot;
            StartCoroutine(matchHelperThis.DoMatch());
            StartCoroutine(matchHelperNeighbour.DoMatch());

            if (!matchHelperThis.canMatch && isBooster && !justCreatedBooster)
            {
                if (type == ItemType.COLOR_BOMB) targetColor = neighbourItem.color;
                DestroyItem(type);
            }

            if (!matchHelperNeighbour.canMatch && neighbourItem.isBooster && !neighbourItem.justCreatedBooster)
            {
                if (neighbourItem.type == ItemType.COLOR_BOMB) neighbourItem.targetColor = color;
                neighbourItem.DestroyItem(neighbourItem.type);
            }

            fieldController.FindMatches();
        }
        else
        {
            backMove = true;
        }

        yield return null;

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
                if (neighbourItem != null)
                    neighbourItem.transform.position = Vector2.Lerp(startPosition, targetPosition, t);
                yield return null;
            }
        }

        isSwapping = false;
        if (neighbourItem) neighbourItem.isSwapping = false;
    }

    public void StartFalling()
    {
        // if (!isFalling)
        StopCoroutine("FallingCoroutine");
        StartCoroutine("FallingCoroutine");
    }

    IEnumerator FallingCoroutine()
    {
        isFalling = true;
        isSwapping = false;

        float startTime = Time.time;
        float duration = .25f;
        float t = 0;
        Vector2 startPosition = transform.position;
        Vector2 targetPosition = slot.transform.position;

        while (t < 1)
        {
            t = (Time.time - startTime) / duration;
            transform.position = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        if (!slot.CanFallNext() && (transform.position == slot.transform.position))
        {
            MatchHelper matchHelper = slot.TryMatch();

            if (matchHelper.canMatch)
            {
                StartCoroutine(matchHelper.DoMatch());

                fieldController.requireCallFallCoroutine = true;
                slot.FallNext();
            }
        }
        else
        {
            isFalling = false;
            slot.FallNext();
        }

        isSwapping = false;
        isFalling = false;
    }

    public void DestroyItem(Slot combineTarget = null, bool isCombine = false)
    {
        slot.item = null;
        if (isCombine && combineTarget)
            StartCoroutine(DestroyItemAnimation(combineTarget, isCombine));
        else
            Destroy(gameObject);
    }

    public void DestroyItem(ItemType targetItemType)
    {
        if (targetItemType != ItemType.NONE && targetItemType != ItemType.DROP)
        {
            if (type == targetItemType)
                ActivateBooster();
            else
                StartCoroutine(ChangeToBoosterAnimation(targetItemType));
        }
        else
        {
            slot.item = null;
            Destroy(gameObject);
        }
    }

    public void ActivateBooster()
    {
        slot.item = null;
        if (type == ItemType.VERTICAL)
            StartCoroutine(DestroyVertical());
        if (type == ItemType.HORIZONTAL)
            StartCoroutine(DestroyHorizontal());
        if (type == ItemType.BOMB)
            StartCoroutine(DestroySquare());
        if (type == ItemType.DYNAMITE)
            StartCoroutine(DestroyRhombus(2));
        if (type == ItemType.BIOHAZARD)
            StartCoroutine(DestroyRhombus(3));
        if (type == ItemType.COLOR_BOMB)
            StartCoroutine(DestroyColor(targetColor));
        if (type == ItemType.AIRPLANE)
        {
            StartCoroutine(DestroyAirplane());
        }
    }

    IEnumerator DestroyVertical()
    {
        fieldController.DestroyVertical(slot.row, slot.col);

        Destroy(gameObject);
        yield return null;
    }

    IEnumerator DestroyHorizontal()
    {
        fieldController.DestroyHorizontal(slot.row, slot.col);

        Destroy(gameObject);
        yield return null;
    }

    IEnumerator DestroySquare()
    {
        fieldController.DestroySquareShape(slot.row, slot.col, 1);

        Destroy(gameObject);
        yield return null;
    }

    IEnumerator DestroyRhombus(int level)
    {
        fieldController.DestroyRhombusShape(slot.row, slot.col, level);

        Destroy(gameObject);
        yield return null;
    }

    IEnumerator DestroyColor(ItemColor targetColor = ItemColor.RANDOM)
    {
        fieldController.DestroyColor(targetColor);

        Destroy(gameObject);
        targetColor = ItemColor.RANDOM;

        yield return null;
    }

    IEnumerator DestroyAirplane()
    {
        fieldController.DestoryRandomAirplaneTarget();

        yield return StartCoroutine(DestroyRhombus(1));

        yield return null;
    }

    IEnumerator DestroyItemAnimation(Slot combineTarget = null, bool isCombine = false)
    {
        float startTime = Time.time;
        float duration = .1f;
        float t = 0;
        Vector2 startPosition = slot.transform.position;
        Vector2 targetPosition = combineTarget.transform.position;

        while (t < 1)
        {
            t = (Time.time - startTime) / duration;
            transform.position = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        Destroy(gameObject);

        fieldController.requireCallFallCoroutine = true;
    }

    IEnumerator ChangeToBoosterAnimation(ItemType targetItemType)
    {
        isFalling = true;
        isBooster = true;
        justCreatedBooster = true;

        float startTime = Time.time;
        float duration = .1f;
        float t = 0;

        while (t < 1)
        {
            t = (Time.time - startTime) / duration;
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            yield return null;
        }

        type = targetItemType;
        if (!GameController.boosterUsingColor) color = ItemColor.RANDOM;
        spriteRenderer.sprite = fieldController.GetSpriteOfBomb(targetItemType, color);

        startTime = Time.time;
        t = 0;

        while (t < 1)
        {
            t = (Time.time - startTime) / duration;
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            yield return null;
        }

        isFalling = false;
        isSwapping = false;
        justCreatedBooster = false;
        fieldController.requireCallFallCoroutine = true;

        yield return null;

        slot.FallNext();
    }

    public bool isDropItem()
    {
        return type == ItemType.DROP;
    }

    public bool isBoosterHasColor()
    {
        if (type == ItemType.VERTICAL && GameController.boosterUsingColor) return true;
        if (type == ItemType.HORIZONTAL && GameController.boosterUsingColor) return true;
        return false;
    }

    public bool isNone()
    {
        return type == ItemType.NONE;
    }

    public void StopFallingDown()
    {
        isFalling = false;
        StopCoroutine("FallingCoroutine");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
