using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ubiq.Avatars.Rocketbox
{
    public class RocketboxMenus
    {
        [MenuItem("Assets/Ubiq/Create Rocketbox Config")]
        private static void CreateRocketboxConfigAction()
        {
            var prefab = Selection.activeGameObject;
            RocketboxEditor.CreateSettingsAsset(prefab);
        }

        [MenuItem("Assets/Ubiq/Create Rocketbox Config", true)]
        private static bool CreateRocketboxConfigValidation()
        {
            return Selection.activeObject is GameObject;
        }

        [MenuItem("Assets/Ubiq/Build AssetBundles")]
        private static void BuildAssetBundles()
        {
            BuildPipeline.BuildAssetBundles(Application.dataPath + "/" + "AssetBundles/Win64/", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
            BuildPipeline.BuildAssetBundles(Application.dataPath + "/" + "AssetBundles/Android/", BuildAssetBundleOptions.None, BuildTarget.Android);
            BuildPipeline.BuildAssetBundles(Application.dataPath + "/" + "AssetBundles/WebGL/", BuildAssetBundleOptions.None, BuildTarget.WebGL);
        }
    }
}