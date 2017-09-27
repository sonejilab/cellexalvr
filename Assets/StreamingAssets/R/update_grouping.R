args <- commandArgs(trailingOnly = TRUE)

selectionfile <- args[1]
userfolder <- args[2]
datafolder <- args[3]

library( cellexalvr )
#print("started")
if ( file.exists( file.path( userfolder, 'cellexalObj.RData' )) ){
	load(  file.path( userfolder, 'cellexalObj.RData' ))
#	print("load from user folder")
}else {
	load(  file.path( datafolder, 'cellexalObj.RData' ))
#	print("load from data folder")
}

cellexalObj <- userGrouping( cellexalObj, selectionfile )

t <- exportUserGroups4vr ( cellexalObj, userfolder )

file.copy(selectionfile, file.path( userfolder,paste(sep='.', cellexalObj@usedObj$lastGroup,'txt' ) ) )

lockedSave ( cellexalObj, userfolder)

#print("done")
