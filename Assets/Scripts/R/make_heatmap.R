#print("start R")


library(gplots)

library(pheatmap)

library(cellexalvr)

args <- commandArgs(trailingOnly = TRUE)

homedir <- args[1]

datadir <- args[2]
latest_version <- args[3]


expression_data_filepath <- file.path(datadir, "cellexalObj.RData")

group_selection_filepath <- file.path(homedir, "Assets","Data","runtimeGroups", paste("selection", latest_version, ".txt", sep=""))

print(group_selection_filepath)

generated_image_filepath <- file.path(homedir, "Assets","Images","heatmap.png")

top_genes_number <- 250

make.cellexalvr.heatmap(expression_data_filepath,group_selection_filepath,top_genes_number,generated_image_filepath)