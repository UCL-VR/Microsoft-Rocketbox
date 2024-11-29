using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Ubiq.Avatars.Rocketbox
{
    public class Tutorial : MonoBehaviour
    {
        public RocketboxManager manager;
        public RocketboxAvatar avatar;
        public Camera screenshotCamera;

        // The maximum number of avatars to go through - this can be reduced
        // to 2 or 3 so it doesn't go through the whole list, if this script
        // should be run many times for debugging purposes.

        public int NumAvatarsToCapture  = 1000;

        // When an avatar is loaded, the bone transforms are reset for its
        // skeleton. These arrays save the rotations of the poses we would
        // like to preserve.

        public List<Transform> transforms = new List<Transform>();

        private Dictionary<Transform, Quaternion> rotations = new Dictionary<Transform, Quaternion>();

        private Texture2D screenshotTexture;

        void Start()
        {
            foreach (var transform in transforms)
            {
                rotations.Add(transform, transform.rotation);
            }

            // Set up functionality for taking screenshots.

            screenshotTexture = new Texture2D(screenshotCamera.pixelWidth, screenshotCamera.pixelHeight, TextureFormat.ARGB32, false);
            screenshotCamera.backgroundColor = Color.clear;

            StartCoroutine(AvatarTutorial());
        }

        private IEnumerator AvatarTutorial()
        {
            // Get a lits of all the Avatars to import

            yield return manager.server.DownloadManifest(); // Once this is done, the manifest will have downloaded

            for (int i = 0; i < Mathf.Min(manager.server.manifest.Avatars.Count, NumAvatarsToCapture); i++)
            {
                var name = manager.server.manifest.Avatars[i];

                Debug.Log("Creating headshot for " + name);

                yield return manager.LoadAvatarAsync(name, avatar);

                // Apply the pose

                foreach(var item in rotations)
                {
                    item.Key.rotation = item.Value;
                }

                // The avatar is now loaded (because LoadAvatarAsync is a co-
                // routine). We can now use the Avatar to capture the screenshot.

                yield return new WaitForEndOfFrame();

                TakeScreenshot(name + ".png");
            }
        }

        private void TakeScreenshot(string filename)
        {
            // We need to use this approach instead of Screenshot because that
            // won't write transparent even with background colour set to clear.

            screenshotCamera.Render();

            var tmp = RenderTexture.active;
            RenderTexture.active = screenshotCamera.targetTexture;

            screenshotTexture.ReadPixels(new Rect(0, 0, screenshotTexture.width, screenshotTexture.height), 0, 0);
            var bytes = screenshotTexture.EncodeToPNG();
            System.IO.File.WriteAllBytes(filename, bytes);

            RenderTexture.active = tmp;
        }
    }
}