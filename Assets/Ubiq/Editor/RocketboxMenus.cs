using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ubiq.Avatars.Rocketbox
{
    public class RocketboxMenus : MonoBehaviour
    {
        [MenuItem("Assets/Ubiq/Create Rocketbox Config")]
        private static void CreateRocketboxConfigAction()
        {
            var prefab = Selection.activeGameObject;
            var path = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(prefab));
            var config = RocketboxHelper.CreateSettingsObject(prefab);
            AssetDatabase.CreateAsset(config, path + "/" + prefab.name + ".asset");
            AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(config)).SetAssetBundleNameAndVariant(prefab.name, "unity3d");
        }

        [MenuItem("Assets/Ubiq/Create Rocketbox Config", true)]
        private static bool CreateRocketboxConfigValidation()
        {
            return Selection.activeObject is GameObject;
        }

        [MenuItem("Assets/Ubiq/Build AssetBundles")]
        private static void BuildAssetBundles()
        {
            BuildPipeline.BuildAssetBundles(Application.dataPath + "/" + "AssetBundles/", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        }
    }
}