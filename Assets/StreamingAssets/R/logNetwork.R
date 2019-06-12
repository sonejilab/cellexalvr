args <- commandArgs(trailingOnly = TRUE)

datadir <- args[1] ## <user specific folder>

network_png <- args[2]

grouping <- args[3]

#if ( is.na(genes) ) {
#	genes = NULL
#}
#ontology <- args[5]
#if ( is.na( ontology) ) {
#	ontology = 'BP'
#}
#topNodes  <- args[6] 
#if ( is.na( topNodes) ) {
#	topNodes = 20
#}
function_str <- paste("logNetwork(cellexalObj,
			NULL,
			\"", network_png, "\",
			\"", grouping, "\")", 
			sep="")


fileConn <- file(file.path(datadir, "server.input.R"))
writeLines(function_str, fileConn)
close(fileConn)

