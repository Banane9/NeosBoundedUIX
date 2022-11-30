Dynamic Variable Space Tree
===========================

A [NeosModLoader](https://github.com/zkxs/NeosModLoader) mod for [Neos VR](https://neos.com/) that adds buttons to the DynamicVariableSpace component in the inspector that allow copying all linked variable definitions or the whole hierarchy of linked dynamic variable components. 

## Installation
1. Install [NeosModLoader](https://github.com/zkxs/NeosModLoader).
2. Place [DynVarSpaceTree.dll](https://github.com/Banane9/NeosDynVarSpaceTree/releases/latest/download/DynVarSpaceTree.dll) into your `nml_mods` folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\NeosVR\nml_mods` for a default install. You can create it if it's missing, or if you launch the game once with NeosModLoader installed it will create the folder for you.
3. Start the game. If you want to verify that the mod is working you can check your Neos logs.

## Trimmed Sample Outputs

Linked Variable Definitions:

```
Variables linked to Namespace SpotifyStatus:
MainRim (color)
MainBG (color)
MainGradient (color)
Cover (Uri)
Cover (IAssetProvider<ITexture2D>)
Cover (IAssetProvider<Sprite>)
CoverAccent (color)
Album (String)
Album (Uri)
FontRegular (IAssetProvider<FontSet>)
Title (String)
Title (Uri)
FontBold (IAssetProvider<FontSet>)
WSClient (WebsocketClient)
...
```

Linked Dynamic Variable Components in Hierarchy:

```
Spotify Control 2.1.3: Namespace SpotifyStatus
├─Fixed UI
│ ├─Main
│ │ ├─Background
│ │ │ ├─SpotifyStatus/MainRim (DynamicValueVariableDriver<color>)
│ │ │ │
│ │ │ ├─Background Mask
│ │ │ │ ├─SpotifyStatus/MainBG (DynamicValueVariableDriver<color>)
│ │ │ │ │
│ │ │ │ └─Gradient
│ │ │ │   └─SpotifyStatus/MainGradient (DynamicValueVariableDriver<color>)
│ │ │ │
│ │ │ └─Content
│ │ │   ├─Song Info
│ │ │   │ ├─Cover
│ │ │   │ │ ├─Image
│ │ │   │ │ │ ├─SpotifyStatus/Cover (DynamicField<Uri>)
│ │ │   │ │ │ ├─SpotifyStatus/Cover (DynamicReferenceVariable<IAssetProvider<ITexture2D>>)
│ │ │   │ │ │ └─SpotifyStatus/Cover (DynamicReferenceVariable<IAssetProvider<Sprite>>)
│ │ │   │ │ │
│ │ │   │ │ └─Rim
│ │ │   │ │   └─SpotifyStatus/CoverAccent (DynamicValueVariableDriver<color>)
...
```
