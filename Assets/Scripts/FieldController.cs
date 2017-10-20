using System;
using System.Collections;
using System.Collections.Generic;
using TouchScript.Gestures;
using TouchScript.Hit;
using UnityEngine;

public class FieldController : MonoBehaviour
{

    public GameController gameController;
    public GameObject slotPrefab;
    public GameObject itemPrefab;

    [HideInInspector]
    public Test test;

    MetaGesture metaGesture;

    Slot currentSlotSelected;
    Slot targetSwapSlot;

    Slot[] slotArr;

    [HideInInspector]
    public int maxRow, maxCol;
    [HideInInspector]
    public bool requireCallFallCoroutine;

    bool isSwapping = false;
    int colorVariant;
    Vector2 startTouchPosition, endTouchPosition;
    Vector2 firstSlotPosition;
    float squareWidth;
    float squareHeight;

    void Awake()
    {
        metaGesture = transform.GetComponent<MetaGesture>();

        TextAsset targetFile = Resources.Load<TextAsset>("levelingTestCase");
        TestCase testCase = JsonUtility.FromJson<TestCase>(targetFile.text);
        test = testCase.testCase[5];
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

        GenerateNewItems(false);

        GameController.gameState = GameState.Playing;
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

    void GenerateNewItems(bool falling = true)
    {
        for (int row = 0; row < maxRow; row++)
        {
            for (int col = 0; col < maxCol; col++)
            {
                GenerateSingleNewItems(row, col, falling);
            }
        }
    }

    void GenerateSingleNewItems(int row, int col, bool falling = true)
    {
        Slot slot = GetSlot(row, col);
        if (slot != null && slot.item == null)
        {
            Item item = slot.GenerateItem(falling);
        }
    }

    public void FindMatches()
    {
        StopCoroutine("FallingDown");
        StartCoroutine("FallingDown");
    }

    IEnumerator FallingDown()
    {
        requireCallFallCoroutine = false;

        yield return null;

        while (true)
        {
            for (int row = maxRow - 1; row > -1; row--)
            {
                for (int col = 0; col < maxCol; col++)
                {
                    Slot slot = GetSlot(row, col);
                    if (slot != null)
                    {
                        if (slot.item != null && !slot.item.isFalling)
                            slot.FallNext();
                    }
                }
            }

            int countEmpty = 0;

            for (int col = 0; col < maxCol; col++)
            {
                Slot slot = GetSlot(0, col);
                if (slot != null)
                {
                    if (slot.item == null)
                    {
                        countEmpty++;
                        GenerateSingleNewItems(0, col);
                    }
                }
            }

            if (countEmpty == 0 && IsAllItemsFallDown())
                break;
            yield return null;
        }

        if (requireCallFallCoroutine) FindMatches();
    }

    bool IsAllItemsFallDown()
    {
        if (GameController.gameState == GameState.Playing)
        {
            for (int i = 0; i < slotArr.Length; i++)
                if (slotArr[i].item && slotArr[i].item.isFalling)
                    return false;
        }
        return true;
    }

    public void DestroyVertical(int row, int col)
    {
        for (int i = maxRow - 1; i > -1; i--)
        {
            if (i == row) continue;
            Slot slot = GetSlot(i, col);
            if (slot != null && slot.item != null && !(slot.item.isBooster && slot.item.isFalling))
            {
                slot.item.DestroyItem(slot.item.type);
            }
        }
    }

    public void DestroyHorizontal(int row, int col)
    {
        for (int i = 0 - 1; i < maxCol; i++)
        {
            if (i == col) continue;
            Slot slot = GetSlot(row, i);
            if (slot != null && slot.item != null && !(slot.item.isBooster && slot.item.isFalling))
            {
                slot.item.DestroyItem(slot.item.type);
            }
        }
    }

    public void DestroySquareShape(int row, int col, int range)
    {
        int fromRow = Math.Max(0, row - range);
        int fromCol = Math.Max(0, col - range);
        int toRow = Math.Min(maxRow, row + range);
        int toCol = Math.Min(maxCol, col + range);

        for (int i = toRow; i >= fromRow; i--)
        {
            for (int j = fromCol; j <= toCol; j++)
            {
                if (i == row && j == col) continue;
                Slot slot = GetSlot(i, j);
                if (slot != null && slot.item != null && !(slot.item.isBooster && slot.item.isFalling))
                {
                    slot.item.DestroyItem(slot.item.type);
                }
            }
        }
    }

    public void DestroyRhombusShape(int row, int col, int radius)
    {
        int fromRow = row - radius;
        int fromCol = col - radius;
        int toRow = row + radius;
        int toCol = col + radius;

        // Debug.Log(fromRow + " " + fromCol + " " + toRow + " " + toCol);

        for (int i = radius - 1; i > -1; i--)
        {
            int startIndx = radius - i;
            for (int j = 0; j < i * 2 + 1; j++)
            {
                int tmpCol = fromCol + startIndx + j;
                int tmpRow = toRow - i;
                if (tmpRow == row && tmpCol == col) continue;
                Slot slot = GetSlot(tmpRow, tmpCol);
                if (slot && slot.item && !(slot.item.isBooster && slot.item.isFalling)) slot.item.DestroyItem(slot.item.type);
            }
        }

        for (int i = 0; i <= radius; i++)
        {
            int startIndx = radius - i;
            for (int j = 0; j < i * 2 + 1; j++)
            {
                int tmpCol = fromCol + startIndx + j;
                int tmpRow = fromRow + i;
                if (tmpRow == row && tmpCol == col) continue;
                Slot slot = GetSlot(tmpRow, tmpCol);
                if (slot && slot.item && !(slot.item.isBooster && slot.item.isFalling)) Destroy(slot.item.gameObject);
            }
        }
    }

    public void DestroyColor(ItemColor color)
    {
        StopCoroutine("FallingDown");

        if (color == ItemColor.RANDOM)
            color = (ItemColor)UnityEngine.Random.Range(0, colorVariant);

        for (int i = maxRow - 1; i > -1; i--)
        {
            for (int j = 0; j < maxCol; j++)
            {
                Slot slot = GetSlot(i, j);
                if (slot && slot.item && !(slot.item.isBooster && slot.item.isFalling))
                {
                    slot.item.StopFallingDown();
                    if (slot.item.color == color)
                    {
                        slot.item.DestroyItem(slot.item.type);
                    }
                }
            }
        }
    }

    public void DestoryRandomAirplaneTarget()
    {
        Slot slot = slotArr[UnityEngine.Random.Range(0, slotArr.Length - 1)];

        while (slot == null || slot.item == null || slot.item.isFalling || slot.item.isBooster || (slot.item.type == ItemType.DROP))
        {
            slot = slotArr[UnityEngine.Random.Range(0, slotArr.Length - 1)];
        }

        slot.item.DestroyItem(slot.item.type);
    }

    public void DestroySingleItem(int row, int col)
    {

    }

    void checkPosibleMove()
    {

    }

    void shuffle()
    {

    }

    void showHighlight()
    {

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

    public Vector3 GetTopPosition()
    {
        return new Vector3(0, squareHeight);
    }

    public Sprite GetSpriteOfColor(ItemColor color)
    {
        return gameController.colorSpriteDict[color];
    }

    public Sprite GetSpriteOfBomb(ItemType itemType, ItemColor color)
    {
        if (GameController.boosterUsingColor)
        {
            if (itemType == ItemType.VERTICAL)
            {
                return gameController.VerticalSpriteDict[color];
            }
            else if (itemType == ItemType.HORIZONTAL)
            {
                return gameController.HorizontalSpriteDict[color];
            }
        }

        return gameController.typeSpriteDict[itemType];
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            DestroyRhombusShape(3, 3, 2);
        }
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
        if (tmpSlotSelected.item != null && (tmpSlotSelected.item.isSwapping || tmpSlotSelected.item.isFalling)) return;
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
            if (currentSlotSelected.item.isBooster && !GameController.boosterUsingColor)
            {
                currentSlotSelected.item.ActivateBooster();
                currentSlotSelected = null;
                FindMatches();
            }
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
            if (!(targetSlot == null || (targetSlot.item != null && (targetSlot.item.isSwapping || targetSlot.item.isFalling))))
                currentSlotSelected.item.Swap(targetSlot);
            currentSlotSelected = null;
        }
        // Debug.Log("Swap");
    }
}
