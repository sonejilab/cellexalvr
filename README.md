# CellexalVR
A tool for visualising and analysing single cell expression data in VR.
CellexalVR was created by some masters students at Lund's University in cooperation with some scientists at the biomedical centre.
Its purpose is to create a more intuitive way of performing single cell analysis on large datasets.

## Running CellexalVR
There are a few things that must be done if you are planning to run CellexalVR for the first time.
* Make sure [R 3.4.0](https://cran.r-project.org/src/base/R-3/) is installed.
* Create a Config folder where CellexalVR is installed.
* Create a config.txt within the Config folder and put the full path to your RScript.exe in it. E.g. C:\Program Files\R\R-3.4.0\bin\x64\Rscript.exe
* Create a Data folder where CellexalVR is installed.
* Put your data inside a new folder with a sensible name in the Data folder.
* If you have multiple sets of data, place them into one folder each within the Data folder.
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
### Requirments
* A pretty beefy computer
* An HTC vive
* [Unity 5.6.1f1](https://unity3d.com/get-unity/download/archive)
* [R 3.4.0](https://cran.r-project.org/src/base/R-3/)
