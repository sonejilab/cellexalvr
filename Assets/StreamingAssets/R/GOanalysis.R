#suppressMessages(library(cellexalvrR))
args <- commandArgs(trailingOnly = TRUE)

datadir <- args[1] ## please give me the user spcific analysis path here!!!!

genes <- args[2] ## the heatmap_<x>.txt file

grouping <- args[3] ## groupings file

#ontology <- args[4]
#if ( is.na( ontology) ) {
#	ontology = 'BP'
#}

#topNodes  <- args[5] 

#if ( is.na( topNodes) ) {
#	topNodes = 10
#}


function_str <- paste("ontologyLogPage(cellexalObj,\"", genes , "\", \"", grouping, "\")", sep="")

fileConn <- file(file.path(datadir, "mainServer.input.R"))
writeLines(function_str, fileConn)
close(fileConn)

#ontologyLogPage(cellexalObj, genes, grouping, ontology = ontology, topNodes = topNodes )
