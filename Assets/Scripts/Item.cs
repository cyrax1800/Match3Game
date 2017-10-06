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
    public bool isFalling;

    Slot neighbourSlot;
    Item neighbourItem;

    Item nextTarget;

    void Awake()
    {
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

        if (matchHelperThis.canMatch || matchHelperNeighbour.canMatch)
        {
            if (neighbourItem != null)
                neighbourItem.slot = slot;
            slot = neighbourSlot;
            StartCoroutine(matchHelperThis.DoMatch());
            StartCoroutine(matchHelperNeighbour.DoMatch());

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
        if (!isFalling)
            StartCoroutine(FallingCoroutine());
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

        if (!slot.CanFallNext())
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
        if (isCombine)
            StartCoroutine(DestroyItemAnimation(combineTarget, isCombine));
        else
            Destroy(gameObject);
    }

    public void DestroyItem(int totalMatch = 0)
    {
        if (totalMatch >= 3)
            StartCoroutine(ChangeToBoosterAnimation());
        else
        {
            slot.item = null;
            Destroy(gameObject);
        }
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

    IEnumerator ChangeToBoosterAnimation()
    {

        float startTime = Time.time;
        float duration = .1f;
        float t = 0;

        while (t < 1)
        {
            t = (Time.time - startTime) / duration;
            yield return null;
        }

        yield return null;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
