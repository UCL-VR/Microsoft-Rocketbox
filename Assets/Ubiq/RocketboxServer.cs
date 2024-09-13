using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

namespace Ubiq.Avatars.Rocketbox
{
    [CreateAssetMenu(fileName = "Rocketbox Avatars", menuName = "Ubiq/Rocketbox/Server Settings")]
    public class RocketboxServer : ScriptableObject
    {
        public string URI = "nexus.cs.ucl.ac.uk/rocketbox/AssetBundles";

        [NonSerialized]
        public RocketboxManifest manifest;

        public string ResolveURI(string avatarName)
        {
            return URI + "/" + RocketboxHelper.RuntimePlatformToBuildTarget(Application.platform).ToString() + "/" + avatarName + ".unity3d";
        }

        public IEnumerator DownloadManifest()
        {
            var uri = URI + "/manifest.json";
            var request = UnityWebRequest.Get(uri);
            yield return request.SendWebRequest();
            if(request.result == UnityWebRequest.Result.Success)
            {
                 manifest = JsonUtility.FromJson<RocketboxManifest>(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError(request.result);
            }
        }
    }
}