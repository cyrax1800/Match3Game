using System;
using System.Collections;
using System.Collections.Generic;
using TouchScript.Gestures;
using TouchScript.Hit;
using UnityEngine;

public class FieldController : MonoBehaviour
{

    public enum Direction
    {
        NONE = -1,
        UP,
        DOWN,
        LEFT,
        RIGHT
    }

    public GameController gameController;
    public GameObject slotPrefab;
    public GameObject itemPrefab;

    MetaGesture metaGesture;

    Slot currentSlotSelected;
    Slot targetSwapSlot;

    Slot[] slotArr;

    Dictionary<Utilities.Color, Sprite> colorSpriteDict = new Dictionary<Utilities.Color, Sprite>();
    bool isSwapping = false;
    int maxRow, maxCol;
    int colorVariant;
    Vector2 startTouchPosition, endTouchPosition;
    Vector2 firstSlotPosition;
    float squareWidth;
    float squareHeight;

    void Awake()
    {
        metaGesture = transform.GetComponent<MetaGesture>();
    }

    // Use this for initialization
    void Start()
    {

    }

    public void Init()
    {
        this.maxRow = gameController.maxRow;
        this.maxCol = gameController.maxCol;
        this.colorVariant = gameController.colorVariant;

        for (int i = 0; i < gameController.colorSprites.Length; i++)
            colorSpriteDict.Add(gameController.colorSprites[i].color, gameController.colorSprites[i].sprite);

        slotArr = new Slot[maxRow * maxCol];

        Vector2 slotSize = slotPrefab.GetComponent<SpriteRenderer>().bounds.size;
        squareWidth = slotSize.x;
        squareHeight = slotSize.y;

        firstSlotPosition = new Vector2(transform.position.x - squareWidth * (float)(maxCol / 2f) + squareWidth / 2, transform.position.y + squareHeight * (float)(maxRow / 2f) - squareHeight / 2);

        for (int row = 0; row < maxRow; row++)
        {
            for (int col = 0; col < maxCol; col++)
            {
                CreateSlot(row, col);
            }
        }

        GenerateNewItems();
    }

    void CreateSlot(int row, int col)
    {
        GameObject slotGO = null;
        slotGO = Instantiate(slotPrefab, firstSlotPosition + new Vector2(col * squareWidth, -row * squareHeight), Quaternion.identity) as GameObject;
        slotGO.transform.SetParent(transform);
        Slot slot = slotGO.GetComponent<Slot>();
        slot.row = row;
        slot.col = col;
        slot.fieldController = this;
        slotArr[row * maxRow + col] = slot;
    }

    void InitLevelItem()
    {

    }

    void GenerateNewItems()
    {
        for (int row = 0; row < maxRow; row++)
        {
            for (int col = 0; col < maxCol; col++)
            {
                if (GetSlot(col, row) != null)
                {
                    Item slot = GetSlot(row, col).GenerateItem();
                }
            }
        }
    }

    public Slot GetSlot(int row, int col, bool safe = false)
    {
        if (!safe)
        {
            if (row >= maxRow || col >= maxCol || row < 0 || col < 0)
                return null;
            return slotArr[row * maxRow + col];
        }
        else
        {
            row = Mathf.Clamp(row, 0, maxRow - 1);
            col = Mathf.Clamp(col, 0, maxCol - 1);
            return slotArr[row * maxCol + col];
        }
    }

    public Sprite GetSpriteOfColor(Utilities.Color color)
    {
        return colorSpriteDict[color];
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        metaGesture.TouchBegan += OnTouchBegan;
        metaGesture.TouchEnded += OnTouchEnd;
    }

    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// </summary>
    void OnDisable()
    {
        metaGesture.TouchBegan -= OnTouchBegan;
        metaGesture.TouchEnded -= OnTouchEnd;
    }

    public void OnTouchBegan(object obj, MetaGestureEventArgs e)
    {
        MetaGesture gesture = (obj as MetaGesture);

        startTouchPosition = e.Touch.Position;

        TouchHit touchHit;
        gesture.GetTargetHitResult(out touchHit);
        Slot tmpSlotSelected = touchHit.RaycastHit2D.collider.gameObject.GetComponent<Slot>();
        // Debug.Log("Began");
        if (currentSlotSelected == null && tmpSlotSelected.item == null) return;
        if (tmpSlotSelected.item != null && tmpSlotSelected.item.isSwapping) return;
        if (currentSlotSelected == null)
        {
            // Debug.Log("Click");
            // TODO: untuk obstacle yang lain, handle disini juga
            if (tmpSlotSelected.item != null) // Jika slot yang di klik adalah slot kosong
                currentSlotSelected = tmpSlotSelected;
        }
        else if (currentSlotSelected == tmpSlotSelected)
        {
            // Double Click
            // TODO: jika dia booster maka harus ada waktu untuk cancel double click
            // Debug.Log("Double Click");
        }
        else if (currentSlotSelected != tmpSlotSelected)
        {
            if (Utilities.IsNeighbour(currentSlotSelected, tmpSlotSelected))
            {
                // Swap
                // TODO: Handle Touch End
                // Debug.Log("Swap");
                currentSlotSelected.item.Swap(tmpSlotSelected);
            }
            else
            {
                // Debug.Log("Not Neighbour, Then return");
            }
            currentSlotSelected = null;
        }
    }

    public void OnTouchEnd(object obj, MetaGestureEventArgs e)
    {
        if (currentSlotSelected == null) return;
        if (currentSlotSelected.item == null) return;

        MetaGesture gesture = (obj as MetaGesture);

        endTouchPosition = e.Touch.Position;

        Vector2 deltaPosition = endTouchPosition - startTouchPosition;

        Direction direction = Utilities.GetDirectionGesture(deltaPosition);
        // Debug.Log(direction.ToString());
        Slot targetSlot = currentSlotSelected.GetNeighBour(direction);

        if (direction != Direction.NONE)
        {
            //Swap
            if (targetSlot == null) return;
            // Debug.Log("Swap");
            currentSlotSelected.item.Swap(targetSlot);
            currentSlotSelected = null;
        }


    }
}
