#suppressMessages(library(cellexalvrR))

args <- commandArgs(trailingOnly = TRUE)

datadir <- args[1] ## <user specific folder>

genes <- args[2] ## the heatmap_<x>.txt file

heatmap_png <- args[3] ## the heatmap figure file

grouping <- args[4] ## the grouping info selection0.txt or so

#message(paste( "log Heatmap with grouping file ", grouping) )

ontology <- args[5]

if ( is.na( ontology) ) {
	ontology = 'BP'
}

topNodes  <- args[6] 

if ( is.na( topNodes) ) {
	topNodes = 20
}

function_str <- paste("logHeatmap(cellexalObj,
			\"", genes , "\",
			\"", heatmap_png, "\",
			\"", grouping, "\"," ,
			ontology, ",",
			topNodes, ")", 
			sep="")


fileConn <- file(file.path(datadir, "mainServer.input.R"))
writeLines(function_str, fileConn)
close(fileConn)
