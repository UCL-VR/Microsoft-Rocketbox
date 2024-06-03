using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Ubiq.Avatars.Rocketbox
{
    public class RocketboxHelper : MonoBehaviour
    {
        public static RocketboxAvatarSettings CreateSettingsObject(GameObject prefab)
        {
            var settings = ScriptableObject.CreateInstance<RocketboxAvatarSettings>();

            var renderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
            var skeleton = prefab.transform.Find("Bip01");

            settings.mesh = renderer.sharedMesh;
            settings.materials.AddRange(
                renderer.sharedMaterials.Select(m => {
                    var materialSettings = new RocketboxAvatarSettings.MaterialSettings();
                    materialSettings.mode = (int)m.GetFloat("_Mode");
                    materialSettings.albedo = m.GetTexture("_MainTex") as Texture2D;
                    materialSettings.normal = m.GetTexture("_BumpMap") as Texture2D;
                    return materialSettings;
                })
            );

            settings.skeleton.AddRange(
                Flatten(skeleton).Select(b => {
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
            var bundle = (request.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
            yield return LoadFromAssetBundleAsync(bundle, avatar);
        }
    }
}