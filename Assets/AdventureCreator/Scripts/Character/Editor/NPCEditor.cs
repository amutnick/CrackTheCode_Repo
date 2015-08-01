using UnityEngine;
using UnityEditor;
using System.Collections;
using AC;

[CustomEditor (typeof (NPC))]

public class NPCEditor : CharEditor
{

	protected override void ExtraSettings ()
	{
		NPC _target = (NPC) target;

		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.LabelField ("NPC settings:", EditorStyles.boldLabel);
		_target.moveOutOfPlayersWay = EditorGUILayout.Toggle ("Keep out of Player's way?", _target.moveOutOfPlayersWay);
		if (_target.moveOutOfPlayersWay)
		{
			_target.minPlayerDistance = EditorGUILayout.FloatField ("Min. distance to keep:", _target.minPlayerDistance);
		}
		EditorGUILayout.EndVertical ();
	}

}
