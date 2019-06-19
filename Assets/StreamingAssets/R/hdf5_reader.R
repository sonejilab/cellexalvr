args <- commandArgs(trailingOnly = TRUE)

datadir <- args[1]

gene_name <- args[2]

output_filepath <- args[3]

library(rhdf5)

