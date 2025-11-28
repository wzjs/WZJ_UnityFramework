using UnityEditor;
using UnityEngine;
using System;

[CustomEditor(typeof(InitialBoard))]
public class InitialBoardEditor : Editor
{
    private InitialBoard board;
    private static readonly string[] gemTypeNames = new string[]
    {
        "Normal1","Normal2","Normal3","Normal4",
        "RowBomb","ColBomb","Rainbow","None"
    };

    private void OnEnable()
    {
        board = target as InitialBoard;
        board.EnsureSize();
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.LabelField("Board Size", EditorStyles.boldLabel);
        int w = EditorGUILayout.IntField("Width", board.width);
        int h = EditorGUILayout.IntField("Height", board.height);
        if (w != board.width || h != board.height)
        {
            board.width = Mathf.Max(1, w);
            board.height = Mathf.Max(1, h);
            board.EnsureSize();
            EditorUtility.SetDirty(board);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Cells", EditorStyles.boldLabel);
        int cols = board.width;
        int rows = board.height;
        for (int y =0; y < rows; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x =0; x < cols; x++)
            {
                int idx = y * cols + x;
                int val = board.cells[idx];
                if (val <0 || val >= gemTypeNames.Length) val =0;
                int selected = EditorGUILayout.Popup(val, gemTypeNames, GUILayout.Width(80));
                board.cells[idx] = selected;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Fill Random"))
        {
            var rand = new System.Random();
            int maxType = board.normalGemTypes; // only normal types
            for (int i =0; i < board.cells.Length; i++) board.cells[i] = rand.Next(0, maxType);
            EditorUtility.SetDirty(board);
        }

        if (GUILayout.Button("Apply To Scene Board"))
        {
            // find a component named Match3Board in scene without compile-time dependency
            var all = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
            foreach (var mb in all)
            {
                var t = mb.GetType();
                if (t.Name == "Match3Board")
                {
                    var method = t.GetMethod("ApplyInitialBoard");
                    if (method != null)
                    {
                        method.Invoke(mb, new object[] { board });
                        Debug.Log("Applied InitialBoard to scene Match3Board.");
                    }
                    else
                    {
                        Debug.LogWarning("Match3Board found but has no ApplyInitialBoard method.");
                    }
                    break;
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
