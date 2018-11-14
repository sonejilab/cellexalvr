suppressMessages(library(cellexalvrR))
args <- commandArgs(trailingOnly = TRUE)

datadir <- args[1] ## please give me the user spcific analysis path here!!!!

genes <- args[2] ## the heatmap_<x>.txt file

grouping <- args[3] ## groupings file

ontology <- args[4]
if ( is.na( ontology) ) {
	ontology = 'BP'
}

topNodes  <- args[5] 

if ( is.na( topNodes) ) {
	topNodes = 10
}
cellexalObj <- loadObject(file.path(datadir, "cellexalObj.RData"))
message("Start ontology Log")
ontologyLogPage(cellexalObj, genes, grouping, ontology = ontology, topNodes = topNodes )
message("End ontology Log")