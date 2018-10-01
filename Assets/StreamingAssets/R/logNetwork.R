library(cellexalvrR)

args <- commandArgs(trailingOnly = TRUE)

datadir <- args[1] ## please give me the user spcific analysis path here!!!!

heatmap_png <- args[2]

grouping <- args[3]

genes <- args[4] ## the heatmap_<x>.txt file

if ( is.na(genes) ) {
	genes = NULL
}

ontology <- args[5]

if ( is.na( ontology) ) {
	ontology = 'BP'
}

topNodes  <- args[6] 

if ( is.na( topNodes) ) {
	topNodes = 20
}

cellexalObj <- loadObject(file.path(datadir, "cellexalObj.RData"))

logNetwork(cellexalObj, genes, heatmap_png, grouping, ontology = ontology, topNodes = topNodes )
