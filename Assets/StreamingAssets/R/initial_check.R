library(cellexalvrR)

args <- commandArgs(trailingOnly = TRUE)

dataSourceFolder <- args[1]

outputFolder <- args[2]

cellexalObj <- loadObject(file.path(dataSourceFolder, "cellexalObj.RData"))



cellexalObj <- renew(cellexalObj)

exportUserGroups4vr(cellexalObj, outputFolder)
