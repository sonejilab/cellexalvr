if ( !is.null(cellexalObj@usedObj$sessionPath) ) {
	message ( "Old session detected - killing old session" )
	cellexalObj@usedObj$sessionPath = NULL
	cellexalObj@usedObj$sessionRmdFiles = NULL
	cellexalObj@usedObj$sessionName = NULL
}

cellexalObj = sessionPath(cellexalObj, NULL)
