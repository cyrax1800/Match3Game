using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    // Only Used in Editor Inspector
    public ColorSpriteField[] colorSprites = new ColorSpriteField[Utilities.GetEnumEntries<Utilities.Color>() - 1];
    //

    public int maxRow = 8;
    public int maxCol = 8;
    public int colorVariant = 4;

    public FieldController fieldController;

    void Awake()
    {

    }

    // Use this for initialization
    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        fieldController.Init();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
