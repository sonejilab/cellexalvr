library(cellexalvrR)

args <- commandArgs(trailingOnly = TRUE)

datadir <- args[1]

gene_name <- args[2]

outputfile <- args[3]

message("In function")

facsTypeArg <- as.logical(args[4])

expression_data_filepath <- file.path(datadir, "cellexalObj.RData")

message("Starting correlated genes function")

get.genes.cor.to(expression_data_filepath, gene_name, outputfile, facsTypeArg)

message("Finished correlated genes function")