using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Ubiq.Avatars.Rocketbox
{
    public class RocketboxHelper
    {
        public static void ApplySettings(RocketboxAvatarSettings settings, RocketboxAvatar avatar)
        {
            var renderer = avatar.GetComponentInChildren<SkinnedMeshRenderer>();
            var skeleton = avatar.transform.Find("Bip01");

            var bonesLookupTable = new Dictionary<string, Transform>();
            foreach (var transform in Flatten(skeleton))
            {
                bonesLookupTable.Add(transform.name, transform);
            }

            foreach (var item in settings.skeleton)
            {
                var transform = bonesLookupTable[item.name];
                transform.localPosition = item.localPosition;
                transform.localRotation = item.localRotation;
            }

            renderer.sharedMesh = settings.mesh;

            var bones = new Transform[settings.bones.Count];
            for (int i = 0; i < settings.bones.Count; i++)
            {
                bones[i] = bonesLookupTable[settings.bones[i]];
            }

            renderer.bones = bones; // Nb this array's entries cannot be updated in-place

            var materials = new Material[settings.materials.Count];

            for (int i = 0; i < settings.materials.Count; i++)
            {
                switch (settings.materials[i].mode)
                {
                    case 0:
                        materials[i] = new Material(avatar.opaqueMaterial);
                        break;
                    case 2:
                        materials[i] = new Material(avatar.fadeMaterial);
                        break;
                    default:
                        throw new System.ArgumentOutOfRangeException();
                }
                materials[i].SetTexture("_MainTex", settings.materials[i].albedo);
                materials[i].SetTexture("_BumpMap", settings.materials[i].normal);
            }
            renderer.sharedMaterials = materials;

            avatar.AvatarLoaded();
        }

        public static IEnumerator LoadFromAssetBundleAsync(AssetBundle bundle, RocketboxAvatar avatar)
        {
            var request = bundle.LoadAllAssetsAsync<RocketboxAvatarSettings>();
            yield return request;
            var settings = request.asset as RocketboxAvatarSettings; // There should only be one per Bundle
            ApplySettings(settings, avatar);
        }

        public static IEnumerator LoadFromUrlAsync(string url, RocketboxAvatar avatar)
        {
            var request = UnityWebRequestAssetBundle.GetAssetBundle(url);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                var bundle = (request.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
                yield return LoadFromAssetBundleAsync(bundle, avatar);
            }
            else
            {
                Debug.LogError(request.result);
            }
        }

        public static IEnumerable<Transform> Flatten(Transform bone)
        {
            yield return bone;
            foreach (Transform child in bone)
            {
                foreach (var b in Flatten(child))
                {
                    yield return b;
                }
            }
        }

        public static BuildTarget RuntimePlatformToBuildTarget(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return BuildTarget.StandaloneWindows64;
                case RuntimePlatform.OSXPlayer:
                    return BuildTarget.StandaloneOSX;
                case RuntimePlatform.IPhonePlayer:
                    return BuildTarget.iOS;
                case RuntimePlatform.Android:
                    return BuildTarget.Android;
                case RuntimePlatform.LinuxPlayer:
                    return BuildTarget.StandaloneLinux64;
                case RuntimePlatform.WebGLPlayer:
                    return BuildTarget.WebGL;
            }

            throw new NotSupportedException(platform.ToString());
        }
    }
}