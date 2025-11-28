using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Match3Board))]
public class Match3BoardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Match3Board board = (Match3Board)target;
        if (GUILayout.Button("À¢–¬∆Â≈Ãœ‘ æ"))
        {
            board.RefreshBoardView();
        }
    }
}