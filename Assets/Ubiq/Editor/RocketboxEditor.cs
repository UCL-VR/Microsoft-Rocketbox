using UnityEngine;
using UnityEditor;
using System.Linq;
using Ubiq.Avatars.Rocketbox;
using System.IO;

namespace Ubiq.Avatars.Rocketbox
{
    public class RocketboxEditor : RocketboxHelper
    {
        public static string buildPath = "/Build";
        public static string configsPath => buildPath + "/Configs";
        public static string configsSystemPath => Application.dataPath + configsPath;
        public static string configsAssetsPath => "Assets" + configsPath;

        public static void CreateSettingsAsset(GameObject prefab)
        {
            System.IO.Directory.CreateDirectory(configsSystemPath);

            var config = CreateSettingsObject(prefab);
            var asset = configsAssetsPath + "/" + prefab.name + ".asset";

            // Remove the asset if it exists

            if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(asset, AssetPathToGUIDOptions.OnlyExistingAssets)))
            {
                AssetDatabase.DeleteAsset(asset);
                AssetDatabase.Refresh();
            }

            AssetDatabase.CreateAsset(config, asset);
            AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(config)).SetAssetBundleNameAndVariant(prefab.name, "unity3d");
        }

        public static RocketboxAvatarSettings CreateSettingsObject(GameObject prefab)
        {
            var settings = ScriptableObject.CreateInstance<RocketboxAvatarSettings>();

            var renderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
            var skeleton = prefab.transform.Find("Bip01");

            settings.mesh = renderer.sharedMesh;
            settings.materials.AddRange(
                renderer.sharedMaterials.Select(m =>
                {
                    var materialSettings = new RocketboxAvatarSettings.MaterialSettings();
                    materialSettings.mode = (int)m.GetFloat("_Mode");
                    materialSettings.albedo = m.GetTexture("_MainTex") as Texture2D;
                    materialSettings.normal = m.GetTexture("_BumpMap") as Texture2D;
                    return materialSettings;
                })
            );

            settings.skeleton.AddRange(
                Flatten(skeleton).Select(b =>
                {
                    var boneSettings = new RocketboxAvatarSettings.BoneSettings();
                    boneSettings.localPosition = b.localPosition;
                    boneSettings.localRotation = b.localRotation;
                    boneSettings.name = b.name;
                    return boneSettings;
                })
            );

            settings.bones.AddRange(renderer.bones.Select(b => b.name));

            return settings;
        }

        public static void BuildManifest()
        {
            RocketboxManifest manifest = new RocketboxManifest();
            manifest.Avatars = new System.Collections.Generic.List<string>();
            foreach (var item in Directory.EnumerateFiles(configsSystemPath))
            {
                if (!item.EndsWith("meta"))
                {
                    manifest.Avatars.Add(Path.GetFileNameWithoutExtension(item).ToLower());
                }
            }
            var json = JsonUtility.ToJson(manifest);
            File.WriteAllText(Application.dataPath + buildPath + "/manifest.json", json);
        }
    }
}