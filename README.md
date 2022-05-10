<img src="https://user-images.githubusercontent.com/277302/166302634-e8435626-2695-4e1c-90b4-323d96bbf3dc.png" width="250px" alt="Pocket Garden Icon" align="right">

# Introduction
<div>Pocket Garden is an app created to show off the ARCore Geospatial API for Google I/O 2022. Locate yourself in the world using your phone to scan your surroundings, and plant seeds to create your own localized garden. Thanks to the Geospatial API your garden will stay right where you left it, even if you close the app. Your garden may have even done a bit of growing while you were away!</div>

## About ARCore Geospatial API
 ARCore Geospatial API is a cloud localization service that allows clients to precisely geolocate smartphones with six degrees of freedom (6dof).</br>
 Unlike GPS, it can provide a **<1m translational and ~1-2 degrees rotational accuracy.** This is accomplished through visually matching your surroundings in streetview covered areas.
 
Dig into the API https://developers.google.com/ar/develop/geospatial

## How to Use This App

Here's a quick rundown on how to use the app:

![Select a seed](https://user-images.githubusercontent.com/8314496/166343401-69e696e5-4e30-45ef-bfd5-006b598603cd.gif)
![Plant](https://user-images.githubusercontent.com/8314496/166343402-f8a0a429-6040-44b0-9764-eacf8bee2b5e.gif)
![Water your plants](https://user-images.githubusercontent.com/8314496/166343403-7e2cc02b-7101-45af-bbfd-9f5cf482f237.gif)
![Restart the app to grow fruit](https://user-images.githubusercontent.com/8314496/166343405-9ff224da-6acf-4468-a69f-c64570b47c9c.gif)
![Shake the plants to harvest the fruit](https://user-images.githubusercontent.com/8314496/166343406-bc16be26-42b5-4f53-bbc5-39812765fd9d.gif)

To look behind the curtain and see the data generated via the Geospatial API, tap the menu button in the upper right and select the globe-shaped icon. Have fun planting!

The Geospatial APi is supported on most devices that run ARCore. To check your specific device visit [https://developers.google.com/ar/devices](https://developers.google.com/ar/devices)

---

## Run Pocket Garden
You can download and run Pocket Garden using this repo.

#### Building Pocket Garden
1. In Google Cloud Console enable the ARCore api https://console.cloud.google.com/apis/library/arcore.googleapis.com
2. Create a Google Cloud Console api key by visiting https://console.cloud.google.com/. Click APIs & Serviecs, and click Credentials.
3. Create a new API key and if you choose to restrict it, make sure you enable the ARCoreAPI
4. Clone this repo and open it in Unity version: 2021.3.1f1 LTS
5. Add your API key in Project Settings > XR Plug-in Management > ARCore Extensions > Android API Key
6. Build and deploy to your Android device
---

# Using This Project

## The Demo Scene
Generally core reuseable classes can be found in the `Assets/_GeoAR Framework` folder, while files specific to this project like UI and plans have been organized into the `Assets/_Pocket Garden` folder.

Load the default Unity scene in the `Assets/_Pocket Garden` folder.
- Note the managers described below on the root Geospatial AR object.

## Code Highlights
#### `GeospatialManager.cs`
The bulk of Geospatial API calls take place in `GeospatialManager` class. You can use this to manage tracking state as well as accuracy. Place an instance on a scene object to begin updating. It inerfaces with all of the common ARCore and ARCore Extension libraires.

#### `InteractionManager.cs`
This is our base class for handling screen to real world interactions for touching `PlaceableObject`s and intersecting ground planes.

#### `PlaceablesManager.cs`
This singleton contains a factory method `CreateGroupAnchor` to create `PlaceablesGroup` objects which store geospatial objects. It also is responsible for saving and loading and generally keeping track of these objects.

#### `PlaceablesGroup.cs`
A PlaceablesGroup is a convenience class to use if you want to create an empty container that will track to a Geospatial Anchor. This is how we keep plants in the same place across different app sessions. To add plants to it, we have plants implement `PlaceableObject`, and we add them to the group with the  `PlaceablesGroup.RegisterPlaceable()` method.

When when the application is closed, PlaceableObject data is serialized and saved as a `GroupData` object, so we can restore it on app launch.

---

# FAQ

<em>My location never localizes</em>
> <p>Make sure you're in an outdoors area with Google Street View coverage, places like public parks may be tougher to localize.  Make sure you also have data service, and not just GPS.</p>

<em>I can't initialize the app</em>
> <p>Make sure you've created an API key in Google Cloud Console and enabled the ARCore API.  A data service is also necessary to use this API.</p>

<em>Can I use the Pocket Garden assets in my own project?</em>
> <p>Yes!</p>
