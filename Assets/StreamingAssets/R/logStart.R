#if ( !is.null(cellexalObj@usedObj$sessionPath) ) {
#	message ( "Old session detected - killing old session" )
#	cellexalObj@usedObj$sessionPath = NULL
#	cellexalObj@usedObj$sessionRmdFiles = NULL
#	cellexalObj@usedObj$sessionName = NULL
#}

args <- commandArgs(trailingOnly = TRUE)

datadir <- args[1] # <user specific folder>

function_str <- paste("cellexalObj <- sessionPath(cellexalObj, NULL)", sep="")

fileConn <- file(file.path(datadir, "mainServer.input.R"))
writeLines(function_str, fileConn)
close(fileConn)