library(cellexalvrR)

args <- commandArgs(trailingOnly = TRUE)

datadir <- args[1]

gene_name <- args[2]

outputfile <- args[3]

facsTypeArg <- as.logical(args[4])

expression_data_filepath <- file.path(datadir, "cellexalObj.RData")

get.genes.cor.to(expression_data_filepath, gene_name, outputfile, facsTypeArg)