suppressMessages(library(cellexalvrR))

args <- commandArgs(trailingOnly = TRUE)

dataSourceFolder <- args[1]

outputFolder <- args[2]
if ( file.exists(file.path(dataSourceFolder, "cellexalObj.RData.lock") ) ){
	## oops - some problems in the last session?
	file.remove(file.path(dataSourceFolder, "cellexalObj.RData.lock") )
}
cellexalObj <- loadObject(file.path(dataSourceFolder, "cellexalObj.RData"))

if ( ! file.exists(outputFolder ) ){
	dir.create( outputFolder )
}
cellexalObj <- renew(cellexalObj)

exportUserGroups4vr(cellexalObj, outputFolder)
