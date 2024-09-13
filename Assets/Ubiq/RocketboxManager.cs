using System.Collections;
using System.Collections.Generic;
using Ubiq.Avatars.Rocketbox;
using UnityEngine;
using UnityEngine.Events;

namespace Ubiq.Avatars.Rocketbox
{
    public class RocketboxManager : MonoBehaviour
    {
        public RocketboxServer server;

        public IEnumerator LoadAvatarAsync(string name, RocketboxAvatar avatar)
        {
            return avatar.LoadFromUrlAsync(server.ResolveURI(name));
        }
    }
}