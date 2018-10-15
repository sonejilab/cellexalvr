suppressMessages(library(cellexalvrR))

args <- commandArgs(trailingOnly = TRUE)

datadir <- args[1] ## please give me the user spcific analysis path here!!!!

genes <- args[2] ## the heatmap_<x>.txt file

heatmap_png <- args[3] ## the heatmap figure file

grouping <- args[4] ## the grouping info selection0.txt or so

message(paste( "log Heatmap with grouping file ", grouping) )

ontology <- args[5]

if ( is.na( ontology) ) {
	ontology = 'BP'
}

topNodes  <- args[6] 

if ( is.na( topNodes) ) {
	topNodes = 20
}

cellexalObj <- loadObject(file.path(datadir, "cellexalObj.RData"))

logHeatmap(cellexalObj, genes, heatmap_png, grouping, ontology = ontology, topNodes = topNodes )

