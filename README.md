# CellexalVR
CellexalVR is a virtual reality platform for the visualisation and analysis of single-cell gene expression data.

Analysing data in virtual reality provides a fully immersive environment where the user can visualise and interact with the data without restriction, with a focus on data visibility, collaboration and cell selection.

You can read more about this project at [our website](https://cellexalvr.med.lu.se).
You can read more about how to use CellexalVR in our [manual](https://cellexalvr.med.lu.se/manual_introduction).

![alt text](https://www.cellexalvr.med.lu.se/images/multiuser_withglobe_s.png)

![alt text](https://www.cellexalvr.med.lu.se/images/velo2.png)

## Running CellexalVR
You can download a compiled version of CellexalVR for Windows 10 from [our website](https://cellexalvr.med.lu.se/download).

There are a few things that must be done if you are planning to run CellexalVR for the first time.
* Check that you have a pretty beefy computer. We are using:
* * intel i7 or AMD ryzen 1920x CPU (SteamVR recommends Intel Core i5-4590/AMD FX 8350 equivalent or better) 
* * NVIDIA GeForce GTX 1080 GPU (SteamVR recommends NVIDIA GeForce GTX 970, AMD Radeon R9 290 equivalent or better)
* * 16 GB RAM (SteamVR recommends 4 GB minimum)
* * 1x USB 2.0 or newer, HDMI 1.4, DisplayPort 1.2 or newer
* Make sure you have an HTC Vive that is set up properly.
* Make sure [SteamVR](https://steamcommunity.com/steamvr) is installed. (This should get installed when you configure your Vive for the first time)
* Make sure at least [R 4.0.0](https://cran.r-project.org/src/base/R-4/) is installed.
* Install the [CellexalVR R package](https://www.github.com/sonejilab/cellexalvrr) by running `devtools::install_github("sonejilab/cellexalvrr")` in an R terminal. This requires the `devtools` package, you can install it with `install.packages("devtools")`.
* If you want to make use of the html reports, you need to install [pandoc](https://pandoc.org/).
* Create a folder called `Data` where CellexalVR is installed.
* Put your data inside a new folder with a sensible name in the `Data` folder.
* If you have multiple sets of data, place them into one folder each within the `Data` folder.
* When you start CellexalVR for the first time, enter the path to your `Rscript.exe` in the text box, or adjust the text between the `RscriptFilePath` tags in the `Config/config.xml` file to where you installed R. It should look something like `<RscriptFilePath>C:\Program Files\R\R-3.6.0\bin\x64\Rscript.exe</RscriptFilePath>`.
The folder structure should then be
```
CellexalVR
|- Config
|  |- config.xml
|
|- Data
|  |- data_set_1
|  |  |- (data files)
|  |
|  |- data_set_2
|     |- (data files)
|
|- CellexalVR.exe
```

## Cloning the project
### Unity Versions
* If you not only want to run a working build of CellexalVR but want to edit and make a your own executable you will need to download and install Unity as well. The different Unity versions are many times backwards compatible but not always so we still recommend that you install the version we list below.  
* [Unity 2019.1.8f1](https://unity3d.com/get-unity/download/archive)
* or [Unity 2018.2.11f1](https://unity3d.com/get-unity/download/archive) if you are checking out a commit before [3535f45](https://github.com/shambam/cellexalvr/commit/3535f4519b8f8efa2edc37f587a2f543a972e8bb)
* or [Unity 2017.3.1f1](https://unity3d.com/get-unity/download/archive) if you are checking out a commit before [5c9324f](https://github.com/shambam/cellexalvr/commit/5c9324f745c802c3b070b9efdcd2a9e0b0428c2a)
* or [Unity 5.6.1f1](https://unity3d.com/get-unity/download/archive) if you are checking out a commit before [0dd8cf8](https://github.com/shambam/cellexalvr/commit/0dd8cf8f2d382f604dd5cca9ba50c2ec73086284)

### Some examples and a more in depth guide of the project is available [here](https://cellexalvr.med.lu.se/programmers-guide).

## Project structure
The Unity project contains 3 scenes that CellexalVR uses. The [Assets/Launcher.unity](Assets/Launcher.unity) scene is what the user sees when they start CellexalVR. It contains the menus for choosing single-user, multi-user or the tutorial. The [Assets/IntroTutorialScene.unity](Assets/IntroTutorialScene.unity) scene is the tutorial, it is for the most part a stripped down version of the main scene, [Assets/CellexalVR_Main_Scene.unity](Assets/CellexalVR_Main_Scene.unity). The main scene is where the interesting things happen.

When you open the main scene, there will only be one gameobject with one script attached to it: the [SceneBuilder](Assets/Scripts/General/SceneBuilder.cs). This script is only responsible for building the main scene. Go ahead and press "Auto-populate gameobjects" if all the fields in the inspector are not already filled in. Then press "Build scene" to build the main scene. After this you may remove the SceneBuilder from the scene. It is located in the [Assets/Prefabs](Assets/Prefabs) folder in case you need it again.

The VR headset and controllers are located under the [\[VRTK\]3.3](Assets/Prefabs/Environment/[VRTK]3.3.prefab) gameobject. We use [VRTK](https://vrtoolkit.readme.io/) for most of the interaction logic in CellexalVR and [SteamVR](https://steamcommunity.com/steamvr) for the hardware communication. The script aliases for the controllers are located under the [\[VRTK_Scripts\]](Assets/Prefabs/Environment/[VRTK_Scripts].prefab) gameobject.

The [InputReader](Assets/Prefabs/Environment/InputReader.prefab) gameobject holds the [InputReader](Assets/Scripts/AnalysisLogic/InputReader.cs) script, which reads the data from the CellexalVR R package and calls the appropriate functions to create graphs and transcription factor networks and so on. It also holds the [ReferenceManager](Assets/Scripts/General/ReferenceManager.cs), a script that just holds references to other scripts to make them easier to access.

The [Generators](Assets/Prefabs/Environment/Generators.prefab) and [Managers](Assets/Prefabs/Environment/Managers.prefab) gameobjects hold children with the different generators and managers respectively. The generators are responsible for generating things such as graphs, heatmaps and transcription factor networks. The managers then handle operations that concern all objects of their respective type.

All buttons on the main menu (attached to the [MenuHolder](Assets/Prefabs/Environment/MenuHolder.prefab) gameobject) derives from the [CellexalButton](Assets/Scripts/Menu/Buttons/CellexalButton.cs) script. This script holds the rudimentary functionality that such as swapping the button's sprite when it is hovered.<br>

All the different keyboards in CellexalVR derive from the base class [KeyboardHandler](Assets/Scripts/Interaction/KeyboardHandler.cs).

The [RScriptRunner](Assets/Scripts/AnalysisLogic/RScriptRunner.cs) class contains the functions that call the external R scripts in the CellexalVR R package to generate heatmaps, transcription factor networks and more. This class is not deriving from [MonoBehaviour](https://docs.unity3d.com/ScriptReference/MonoBehaviour.html) and is thus not attached to a gameobject in the scene.

The multi-user functionality is split between the [GameManager](Assets/Scripts/General/GameManager.cs) and [ServerCoordinator](Assets/Scripts/Multiplayer/ServerCoordinator.cs) classes. These use the [Photon Engine](https://www.photonengine.com/pun) to send packages over the internet. The [GameManager](Assets/Scripts/General/GameManager.cs) contains many functions that inform all other connected clients that something happened on their end, and the same thing should happen in all other clients' sessions and the [ServerCoordinator](Assets/Scripts/Multiplayer/ServerCoordinator.cs) contains the function that repeats the same thing for a client.

There is a [CellManager](Assets/Scripts/AnalysisLogic/CellManager.cs) class that holds a list of [Cell](Assets/Scripts/AnalysisLogic/Cell.cs) objects. The idea here was that if you have multiple graphs, they would each contain graphpoints that represents the same cells. An operation that concern a cell should affect all graphs, and thus that functionality was written in this class.

The values of all gene expressions are stored in an sqlite3 database. This database contains all non-zero expression values of all cells and all genes. The [SQLiter](Assets/Prefabs/Environment/SQLiter.prefab) gameobject contains the [SQLite](Assets/Scripts/AnalysisLogic/SQLite.cs) script that handles the queries that are sent to the database.

The folder structure when it comes to prefabs, materials and scripts is hopefully somewhat intuitive. The [Assets/Prefabs](Assets/Prefabs), [Assets/Materials](Assets/Materials) and [Assets/Images](Assets/Images) folders all have a similar set of folders in them like <code>Graphs</code>, <code>Heatmaps</code>, <code>Menu</code> and so on. The [Assets/Scripts](Assets/Scripts) is organised after which namespace scripts are in. All scripts written by us are in the <code>CellexalVR</code> namespace and its subsequent nested namespaces.

## Video Guides
1. [Using the HTC Vive Controller](https://www.youtube.com/watch?v=sc-CdIeGr_8)
2. [Extended Run-through](https://www.youtube.com/watch?v=owC2GlwvL7Q)
3. [Multi-user Setup](https://www.youtube.com/watch?v=kyAL0t4VND8)
4. [Multi-user Session](https://www.youtube.com/watch?v=pUTFBEtijXA)
5. [Loading Data & Using the Menu](https://www.youtube.com/watch?v=0kyynx6zSxc)
6. [Colouring by Gene Expression](https://www.youtube.com/watch?v=_hep2RtiiiA)
7. [Making a Selection](https://www.youtube.com/watch?v=IuXZR0dHOiw)
8. [Generating and Interacting with Heatmaps](https://www.youtube.com/watch?v=elpndTvboX8)
9. [Generating and Interacting with Transcription Factor Networks](https://www.youtube.com/watch?v=zzL_xpkbcRE)
10. [Colouring by FACS Measurement](https://www.youtube.com/watch?v=5VGhyjqRxUk)
11. [Colouring by Attributes/Meta data](https://www.youtube.com/watch?v=XesoZI3JE6Q)

## Notes
If you lose your config.xml there is a sample one at [Assets/StreamingAssets/sample_config.xml](Assets/StreamingAssets/sample_config.xml). If you run CellexalVR without a `config.xml` file, the sample one will be copied to `Config/config.xml`.

This software uses code of [FFmpeg](http://ffmpeg.org) licensed under the [LGPLv2.1](http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html) and its source can be downloaded [here](https://www.ffmpeg.org/download.html#get-sources).
