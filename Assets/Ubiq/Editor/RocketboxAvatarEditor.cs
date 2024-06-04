using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Ubiq.Avatars.Rocketbox
{
    [CustomEditor(typeof(RocketboxAvatar))]
    public class RocketboxAvatarEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var component = target as RocketboxAvatar;            

            if (GUILayout.Button("Load from URL"))
            {
                component.LoadFromUrl(@"http://192.168.1.2:8080/female_adult_01.unity3d");
            }
        }
    }
}