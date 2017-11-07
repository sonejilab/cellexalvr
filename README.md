# CellexalVR
A tool for visualising and analysing single cell expression data in VR.
CellexalVR was created by some masters students at Lund's University in cooperation with some scientists at the biomedical centre.
Its purpose is to create a more intuitive way of performing single cell analysis on large datasets.
You can read more about this project at [our website](http://cellexalvr.lu.se).

## Running CellexalVR
There are a few things that must be done if you are planning to run CellexalVR for the first time.
* Make sure you have an HTC Vive that is set up properly.
* Make sure [SteamVR](https://steamcommunity.com/steamvr) is installed. (This should get installed when you configure your Vive for the first time)
* Make sure [R 3.4.0](https://cran.r-project.org/src/base/R-3/) is installed.
* Create a Data folder where CellexalVR is installed.
* Put your data inside a new folder with a sensible name in the Data folder.
* If you have multiple sets of data, place them into one folder each within the Data folder.
* Adjust the `RscriptFilePath` in the `Config/config.txt` file to where you installed R. It should look something like `RscriptFilePath = C:\Program Files\R\R-3.4.0\bin\x64\Rscript.exe`
The folder structure should then be
```
CellexalVR
  Config
    config.txt
  Data
    data_set_1
      (data files)
    data_set_2
      (data files)
  CellexalVR.exe
```

## Cloning the project
### Requirements
* A pretty beefy computer
* [Unity 5.6.1f1](https://unity3d.com/get-unity/download/archive)

## Notes
If you lose your config.txt there is a sample one at `Assets/StreamingAssets/sample_config.txt`. If you run the program without a `config.txt` file, the sample one will be copied to `Config/config.txt`.
