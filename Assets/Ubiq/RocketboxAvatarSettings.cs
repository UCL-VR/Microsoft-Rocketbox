using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ubiq.Avatars.Rocketbox
{
    /// <summary>
    /// This class is shared with the Ubiq Sample project, and must have the
    /// exact same definition as there.
    /// </summary>
    public class RocketboxAvatarSettings : ScriptableObject
    {
        [Serializable]
        public class MaterialSettings
        {
            public int mode;
            public Texture2D albedo;
            public Texture2D normal;
        }

        [Serializable]
        public class BoneSettings
        {
            public string name;
            public Vector3 localPosition;
            public Quaternion localRotation;
        }

        public RocketboxAvatarSettings()
        {
            materials = new List<MaterialSettings>();
            skeleton = new List<BoneSettings>();
            bones = new List<string>();
        }

        public Mesh mesh;
        public List<MaterialSettings> materials;
        public List<BoneSettings> skeleton;
        public List<string> bones;
    }
}