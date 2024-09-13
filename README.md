## Ubiq x Rocketbox

This project is for building streamable assets of the Microsoft Rocketbox avatars for Ubiq.

For information about the Rocketbox project, please see the repository upstream.

This is not the project that contains the Ubiq integration for Rocketbox - that is the ubiq-rocketbox fork.

The purpose of this project is to create an asset that contains the differences between a Rocketbox Avatar, and a Template Avatar, which can be applied at runtime. Users request Rocketbox Avatars in the form of Unity AssetBundles and stream these into their process, where the specifics of a given avatar are unpacked and applied to the Template.

The Template Avatar remains the same GameObject throughout its lifetime, allowing users to create Prefab Variants and attach items to its skeleton, if desired.

To use this project, create Configurations for the chosen Rocketbox Avatars, and build the Asset Bundles for the project. These can then be server from a simple static web server.

Tips
1. The menu item to create a Configuration for a single Avatar can be found by right-clicking, then Ubiq -> Create Rocketbox Config. The Config will be created in the same directory, and be tagged with its own AssetBundle.
2. AssetBundles are written to the Assets/AssetBundles folder.
3. Use a development server such as [http-server](https://www.npmjs.com/package/http-server) to serve this folder. For example, `npx http-server .`
4. The Dynamic Avatar has an Editor script that allows loading a URL from the Editor for debugging.