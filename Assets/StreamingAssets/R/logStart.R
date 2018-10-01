library(cellexalvrR)

args <- commandArgs(trailingOnly = TRUE)

datadir <- args[1] ## please give me the user spcific analysis path here!!!!

sessionString <- args[2]

if ( is.na(sessionString ) ){
	sessionString = NULL
}

cellexalObj <- loadObject(file.path(datadir, "cellexalObj.RData"))

cellexalObj = sessionPath(cellexalObj, sessionString )
lockedSave(cellexalObj)
