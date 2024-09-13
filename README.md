## Ubiq x Rocketbox

This project is for building streamable assets of the Microsoft Rocketbox avatars for Ubiq.

For information about the Rocketbox project, please see the repository upstream.

This is not the project that contains the Ubiq integration for Rocketbox - that is the ubiq-rocketbox fork.

The purpose of this project is to create an asset that contains the differences between a Rocketbox Avatar, and a Template Avatar, which can be applied at runtime. Users request Rocketbox Avatars in the form of Unity AssetBundles and stream these into their process, where the specifics of a given avatar are unpacked and applied to the Template.

The Template Avatar remains the same GameObject throughout its lifetime, allowing users to create Prefab Variants and attach items to its skeleton, if desired.

To use this project, create Configurations for the chosen Rocketbox Avatars, and build the Asset Bundles for the project. These can then be served from a simple static web server.

Tips
1. The menu item to create a Configuration for a single Avatar can be found by right-clicking, then Ubiq -> Create Rocketbox Config. The Config will be created in the same directory, and be tagged with its own AssetBundle.
2. AssetBundles are written to the Assets/AssetBundles folder.
3. Use a development server such as [http-server](https://www.npmjs.com/package/http-server) to serve this folder locally for development. For example, `npx http-server .`


## Build Tools

The Build Tools window is opened from Ubiq -> Rocketbox Avatars Build Tools

Here you can choose a combination of Avatars and platforms to build for.

### Create Configs

The "Configs" are the actual Assets that are compiled into AssetBundles and delivered to client applications. One Config exists per Avatar, and one AssetBundle exists per config.

Configs are written into the 'Assets/Build/Configs' folder. Building Configs replaces the originals.

This directory is hardcoded in one place in the RocketboxEditor script.

Building the Config assets is a short operation - a minute or so for all avatars - so there are few filter options.


### Build AssetBundles

Once the Configs have been constructed, the AssetBundles can be built. 

Config Asset files are tagged with the AssetBundle they should belong (each one has its own Bundle), so any way to get Unity to build AssetBundles will cause the AssetBundles for the Configs to built.

The Rocketbox Avatar Window has an option to build them, which will output them into the Build/AssetBundles/[platform] folder.

Building AssetBundles is a longer operation. Between building to different platforms, Unity will reimport all Avatars. Choose the Platforms to build for by adding them to the list.


### Manifest

The manifest file lists all the avatars available. This is generated from the assets list - not the server itself. Build this after building the Configs. 

The manifest file only contains names is mainly a convenience object. To build manifests with additional data, iterate over the assets programmatically - perhaps by downloading AssetBundles - as shown in the Tutorial scene. The resulting metadata is managed and saved per-application.


## Template Scene

There is one Scene in the project, which is used for setting up the Template Avatar Prefab. This scene has a fixed list of avatars that can be loaded from a server at runtime.



### Tutorial

The Rocketbox library is static, meaning the server will hold a list of all avatars, but nothing else.

If your application needs certain metadata or additional assets, they can be safely added programmatically.

The Tutorial scene demonstrates how to generate headshots for each Avatar, using the Loading functionality itself.

This scene consists of a Camera, attached to the Neck bone of the Prototype. The result is that the Camera is always at the same place relative to the face when an Avatar is loaded.

A script, xxxxx, gets a list of Avatars from the Server and enumerates them. After the Avatar is loaded, the script takes a screenshot, and saves it to disk. When all headshots have been taken, the Application exits.


1. The Tutorial script maintains a reference to the RocketboxManager and RocketboxAvatar. The Avatar will be loaded at Runtime. Note how the Avatar is posed already in the scene - it will maintain this pose when a new avatar is loaded.

