using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using MagicP;

[CustomEditor(typeof(FsmDataSync))]
public class FsmDataSyncEditor : Editor
{
	//private int iIndexSource;
	//private int iIndexTarget;
	SerializedObject m_Object;
	//private SerializedProperty sourceFsmName;
	//private SerializedProperty TargetFsmName;
	private SerializedProperty variables;

	void OnEnable()
	{
		m_Object = new SerializedObject(target);
		//sourceFsmName = m_Object.FindProperty("SourceFsmName");
		//TargetFsmName = m_Object.FindProperty("TargetFsmName");
		variables = m_Object.FindProperty("variables");
	}

	public override void OnInspectorGUI()
	{
		//EditorGUILayout.LabelField("Source");
		FsmDataSync scriptObj = (FsmDataSync)target;

		m_Object.Update();

		GameObject obj = scriptObj.gameObject;
		var fsms = obj.GetComponents<PlayMakerFSM>();
		if(fsms.Length <= 0)
		{
			Debug.LogError("object must have playmakerfsm!!!");
			return;
		}
		
		List<string> fsmName = new List<string>();
		for(int i = 0; i < fsms.Length; i++)
		{
			
			fsmName.Add(fsms[i].FsmName);
		}
		
		/*iIndexSource = fsmName.IndexOf(sourceFsmName.stringValue);
		if(iIndexSource < 0)
			iIndexSource = 0;
		iIndexSource = EditorGUILayout.Popup(iIndexSource, fsmName.ToArray(), new GUILayoutOption[] { GUILayout.MaxWidth(100f) });
		sourceFsmName.stringValue = fsmName[iIndexSource];

		source = fsms[iIndexSource];
		EditorGUILayout.LabelField("Target");
		iIndexTarget = fsmName.IndexOf(TargetFsmName.stringValue);
		if(iIndexTarget < 0)
			iIndexTarget = 0;
		iIndexTarget = EditorGUILayout.Popup(iIndexTarget, fsmName.ToArray(), new GUILayoutOption[] { GUILayout.MaxWidth(100f) });
		TargetFsmName.stringValue = fsmName[iIndexTarget];*/

		Debug.Log("target name: " +scriptObj.TargetFsmName);

		EditorGUILayout.LabelField("Variables");
		if(variables.arraySize == 0)
			variables.InsertArrayElementAtIndex(0);
		for(int i = 0; i < variables.arraySize; i++)
		{
			EditorGUILayout.BeginHorizontal();

			SerializedProperty variable = variables.GetArrayElementAtIndex(i);
			SerializedProperty vType = variable.FindPropertyRelative("type");
			FsmDataSync.Type type = (FsmDataSync.Type)vType.intValue;
			type = (FsmDataSync.Type)EditorGUILayout.EnumPopup(type, GUILayout.MaxWidth(80f));
			vType.intValue = (int)type;

            EditorGUILayout.LabelField("Source:", GUILayout.MaxWidth(50f));
            SerializedProperty sourceFsmName = variable.FindPropertyRelative("SourceName");
            int iIndexSource = fsmName.IndexOf(sourceFsmName.stringValue);
            if (iIndexSource < 0)
                iIndexSource = 0;
            iIndexSource = EditorGUILayout.Popup(iIndexSource, fsmName.ToArray(), new GUILayoutOption[] { GUILayout.MaxWidth(80f) });
            sourceFsmName.stringValue = fsmName[iIndexSource];

            PlayMakerFSM source = fsms[iIndexSource];
            List<string> vNames = new List<string>();
			this.GetVariablesName(source, vNames, type);
			if(vNames.Count <= 0)
			{
				vNames.Add("null");
			}
            SerializedProperty svName = variable.FindPropertyRelative("SVariableName");
            int iIndexName = vNames.IndexOf(svName.stringValue);
            if (iIndexName < 0)
                iIndexName = 0;
            iIndexName = EditorGUILayout.Popup(iIndexName, vNames.ToArray(), GUILayout.MaxWidth(50f));
            svName.stringValue = vNames[iIndexName];

            EditorGUILayout.LabelField("Target:", GUILayout.MaxWidth(50f));
            SerializedProperty TargetFsmName = variable.FindPropertyRelative("TargetName");
            int iIndexTarget = fsmName.IndexOf(TargetFsmName.stringValue);
            if (iIndexTarget < 0)
                iIndexTarget = 0;
            iIndexTarget = EditorGUILayout.Popup(iIndexTarget, fsmName.ToArray(), new GUILayoutOption[] { GUILayout.MaxWidth(80f) });
            TargetFsmName.stringValue = fsmName[iIndexTarget];
            PlayMakerFSM targetFsm = fsms[iIndexTarget];
            vNames.Clear();
            this.GetVariablesName(targetFsm, vNames, type);
            if (vNames.Count <= 0)
            {
                vNames.Add("null");
            }
            SerializedProperty tvName = variable.FindPropertyRelative("TVariableName");
            iIndexName = vNames.IndexOf(tvName.stringValue);
            if (iIndexName < 0)
                iIndexName = 0;
            iIndexName = EditorGUILayout.Popup(iIndexName, vNames.ToArray(), GUILayout.MaxWidth(50f));
            tvName.stringValue = vNames[iIndexName];

            if (GUILayout.Button("+", EditorStyles.miniButtonLeft, GUILayout.MaxWidth(20f)))
			{
				variables.InsertArrayElementAtIndex(i);
			}
			if(GUILayout.Button("-", EditorStyles.miniButtonLeft, GUILayout.MaxWidth(20f)))
			{
				variables.DeleteArrayElementAtIndex(i);
			}
			EditorGUILayout.EndHorizontal();
		}

		m_Object.ApplyModifiedProperties();

		//scriptObj.Variables.Insert
//		EditorGUILayout.LabelField("Variables");
//		List<string> variableName = new List<string>();
//		foreach(FsmObject vobj in source.FsmVariables.ObjectVariables)
//		{
//			variableName.Add(vobj.Name);
//		}
	}

	private void GetVariablesName(PlayMakerFSM fsm, List<string> names, MagicP.FsmDataSync.Type type)
	{
		if(type == MagicP.FsmDataSync.Type.INT)
		{
			foreach(FsmInt fi in fsm.FsmVariables.IntVariables)
			{
				names.Add(fi.Name);
			}
		}
	}
}
