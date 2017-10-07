using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(GameController))]
public class GameControllerInspector : Editor
{
    GameController gameController;
    public bool showItemColor = true;
    public bool showBooster = true;
    public bool showHorizontalboosterUsingColor = true;
    public bool showVerticalboosterUsingColor = true;


    void OnEnable()
    {
        gameController = (GameController)target;
    }

    public override void OnInspectorGUI()
    {
        gameController.fieldController = EditorGUILayout.ObjectField("Field Controller", gameController.fieldController, typeof(FieldController), true) as FieldController;
        gameController.maxRow = EditorGUILayout.IntField("Row", gameController.maxRow);
        gameController.maxCol = EditorGUILayout.IntField("Col", gameController.maxCol);

        gameController.colorVariant = EditorGUILayout.IntField("Max Variant Color", gameController.colorVariant);

        EditorGUILayout.Space();

        showItemColor = EditorGUILayout.Foldout(showItemColor, "Item Color", true);
        if (showItemColor)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < Utilities.GetEnumEntries<ItemColor>() - 1; i++)
            {
                if (!gameController.colorSpriteDict.ContainsKey((ItemColor)i))
                    gameController.colorSpriteDict[(ItemColor)i] = null;
                gameController.colorSpriteDict[(ItemColor)i] = EditorGUILayout.ObjectField(((ItemColor)i).ToString().ToTitleCase(), gameController.colorSpriteDict[(ItemColor)i], typeof(Sprite), false, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight)) as Sprite;
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        showBooster = EditorGUILayout.Foldout(showBooster, "Booster", true);
        if (showBooster)
        {
            EditorGUI.indentLevel++;
            for (int i = 1; i < Utilities.GetEnumEntries<ItemType>() - 1; i++)
            {
                if (!gameController.typeSpriteDict.ContainsKey((ItemType)i))
                    gameController.typeSpriteDict[(ItemType)i] = null;
                gameController.typeSpriteDict[(ItemType)i] = EditorGUILayout.ObjectField(((ItemType)i).ToString().ToTitleCase(), gameController.typeSpriteDict[(ItemType)i], typeof(Sprite), false, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight)) as Sprite;
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        GameController.boosterUsingColor = EditorGUILayout.Toggle("Booster using color", GameController.boosterUsingColor);

        if (GameController.boosterUsingColor)
        {
            showVerticalboosterUsingColor = EditorGUILayout.Foldout(showVerticalboosterUsingColor, "Vertical Booster", true);
            if (showVerticalboosterUsingColor)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < Utilities.GetEnumEntries<ItemColor>() - 1; i++)
                {
                    if (!gameController.VerticalSpriteDict.ContainsKey((ItemColor)i))
                        gameController.VerticalSpriteDict[(ItemColor)i] = null;
                    gameController.VerticalSpriteDict[(ItemColor)i] = EditorGUILayout.ObjectField(((ItemColor)i).ToString().ToTitleCase(), gameController.VerticalSpriteDict[(ItemColor)i], typeof(Sprite), false, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight)) as Sprite;
                }
                EditorGUI.indentLevel--;
            }

            showHorizontalboosterUsingColor = EditorGUILayout.Foldout(showHorizontalboosterUsingColor, "Horizontal Booster", true);

            if (showHorizontalboosterUsingColor)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < Utilities.GetEnumEntries<ItemColor>() - 1; i++)
                {
                    if (!gameController.HorizontalSpriteDict.ContainsKey((ItemColor)i))
                        gameController.HorizontalSpriteDict[(ItemColor)i] = null;
                    gameController.HorizontalSpriteDict[(ItemColor)i] = EditorGUILayout.ObjectField(((ItemColor)i).ToString().ToTitleCase(), gameController.HorizontalSpriteDict[(ItemColor)i], typeof(Sprite), false, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight)) as Sprite;
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}