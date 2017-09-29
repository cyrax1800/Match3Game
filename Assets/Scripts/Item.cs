using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{

    public Slot slot;
    public SpriteRenderer spriteRenderer;

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

    void GenColor(Utilities.Color color = Utilities.Color.RANDOM)
    {

        if (color == Utilities.Color.RANDOM)
        {
            color = (Utilities.Color)UnityEngine.Random.Range(0, slot.fieldController.gameController.colorVariant);
        }
        spriteRenderer.sprite = slot.fieldController.GetSpriteOfColor(color);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
