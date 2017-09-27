#print("start R")

args <- commandArgs(trailingOnly = TRUE)

homedir <- args[1]

datadir <- args[2]

latest_version <- args[3]

input_file_dir <- args[4]

library(gplots)

library(pheatmap)

library(cellexalvr)

expression_data_filepath <- file.path(datadir, "cellexalObj.RData")

 group_selection_filepath <- file.path(input_file_dir, paste("selection", latest_version, ".txt", sep=""))

if ( ! file.exists(group_selection_filepath)) {
	group_selection_filepath = latest_version # as this is the grouping name!
}

#print(group_selection_filepath)

generated_image_filepath <- file.path(homedir, "Images", paste("heatmap_", latest_version, ".png", sep=""))

top_genes_number <- 250

cellexalvrObj <- make.cellexalvr.heatmap(expression_data_filepath,group_selection_filepath,top_genes_number,generated_image_filepath)

#this is not necesary any more - done wile the group is stored.
#save ( cellexalvrObj , file = file.path(args[4], "cellexalvrObj.RData", sep="" )) 
