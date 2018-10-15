suppressMessages(library(cellexalvrR))
message( "Start the logging")
args <- commandArgs(trailingOnly = TRUE)

datadir <- args[1] ## please give me the user spcific analysis path here!!!!

sessionString <- args[2]

if ( is.na(sessionString ) ){
	sessionString = NULL
}

cellexalObj <- loadObject(file.path(datadir, "cellexalObj.RData"))

if ( !is.null(cellexalObj@usedObj$sessionPath) ) {
	message ( "Old session detected - killing old session" )
	cellexalObj@usedObj$sessionPath = NULL
	cellexalObj@usedObj$sessionRmdFiles = NULL
	cellexalObj@usedObj$sessionName = NULL
}

cellexalObj = sessionPath(cellexalObj, sessionString )
#lockedSave(cellexalObj)
