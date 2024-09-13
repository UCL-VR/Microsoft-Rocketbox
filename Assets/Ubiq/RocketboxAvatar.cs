using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ubiq.Avatars.Rocketbox
{
    /// <summary>
    /// Configures a Rocketbox Avatar compatible GameObject at startup
    /// </summary>
    public class RocketboxAvatar : MonoBehaviour
    {
        public Material opaqueMaterial;
        public Material fadeMaterial;

        public IEnumerator LoadFromUrlAsync(string url)
        {
            return RocketboxHelper.LoadFromUrlAsync(url, this);
        }

        /// <summary>
        /// Called when a new Avatar has finished loading onto this instance
        /// </summary>
        public void AvatarLoaded()
        {

        }
    }
}