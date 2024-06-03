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
       // public RocketboxAvatarSettings settings;

        public Material opaqueMaterial;
        public Material fadeMaterial;

        // Start is called before the first frame update
        void Start()
        {
            //RocketboxHelper.ApplySettings(settings, this);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void LoadFromUrl(string url)
        {
            StartCoroutine(RocketboxHelper.LoadFromUrlAsync(url, this));
        }
    }
}