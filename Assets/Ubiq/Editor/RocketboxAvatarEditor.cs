using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Ubiq.Avatars.Rocketbox
{
    [CustomEditor(typeof(RocketboxAvatar))]
    public class RocketboxAvatarEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var component = target as RocketboxAvatar;            
        }
    }
}