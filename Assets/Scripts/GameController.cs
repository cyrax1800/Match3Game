using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    // Only Used in Editor Inspector
    public ItemColorSprite colorSpriteDict = new ItemColorSprite();
    public ItemColorSprite VerticalSpriteDict = new ItemColorSprite();
    public ItemColorSprite HorizontalSpriteDict = new ItemColorSprite();
    public ItemTypeSprite typeSpriteDict = new ItemTypeSprite();
    //

    public int maxRow = 8;
    public int maxCol = 8;
    public int colorVariant = 4;
    public static bool boosterUsingColor = false;

    public FieldController fieldController;

    public static GameState gameState = GameState.Start;

    void Awake()
    {

    }

    // Use this for initialization
    void Start()
    {
        GameController.gameState = GameState.Initialize;
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
