#print("start R")

args <- commandArgs(trailingOnly = TRUE)

homedir <- args[1] # <user specific folder>/output

datadir <- args[2] # <user specific folder>

latest_version <- args[3] # filepath to the grouping file

output_filepath <- args[4] # <homedir>/<heatmapName>.txt

top_genes_number <- args[5] # integer norm 250

suppressMessages(library(cellexalvrR))

expression_data_filepath <- file.path(datadir, "cellexalObj.RData")

group_selection_filepath <- file.path(datadir, paste("selection", latest_version, ".txt", sep=""))

if ( ! file.exists(group_selection_filepath)) {
	group_selection_filepath = latest_version # as this is the grouping name!
}

#print(group_selection_filepath)

#generated_image_filepath <- file.path(homedir, "Images", paste("heatmap_", latest_version, ".png", sep=""))

message( paste("make_heatmap using grouping file", file.path(datadir, paste("selection", latest_version, ".txt", sep="") ) ) )

cellexalvrObj <- make.cellexalvr.heatmap.list(expression_data_filepath,group_selection_filepath,top_genes_number,output_filepath)

message( "Heatmap - no object save: grouping should already be stored" )

#this is not necesary any more - done wile the group is stored.
# lockedSave(cellexalObj)
