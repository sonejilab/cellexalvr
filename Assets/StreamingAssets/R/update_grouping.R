args <- commandArgs(trailingOnly = TRUE)

#suppressMessages(library( cellexalvrR ))

#message( paste("update grouping with grouping file",selectionfile ))

#print("started")
#if ( file.exists( file.path( userfolder, 'cellexalObj.RData' )) ){
#	load(  file.path( userfolder, 'cellexalObj.RData' ))
#	print("load from user folder")
#}else {
#	load(  file.path( datafolder, 'cellexalObj.RData' ))
#	print("load from data folder")
#}

datadir <- args[1]
selectionfile <- args[2]


function_str <- paste("cellexalObj <- userGrouping(cellexalObj,
			\"", selectionfile, "\")", 
			sep="")

fileConn <- file(file.path(datadir, "server.input.R"))
writeLines(function_str, fileConn)
close(fileConn)

#t <- exportUserGroups4vr ( cellexalObj, userfolder )

#if ( isS4(cellexalObj) ) {
#	file.copy(selectionfile, file.path( userfolder,paste(sep='.', cellexalObj@usedObj$lastGroup,'txt' ) ) )
#}else {
#	file.copy(selectionfile, file.path( userfolder,paste(sep='.', cellexalObj$usedObj$lastGroup,'txt' ) ) )
#}
#message( "Save updated cellexalvrR object" )
#lockedSave ( cellexalObj, path= userfolder)

#print("done")
