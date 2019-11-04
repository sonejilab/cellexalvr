args <- commandArgs(trailingOnly = TRUE)

datadir <- args[1] # <user specific folder>

group_selection_filepath  <- args[2] # filepath to the grouping file

top_genes_number <- args[3] # integer norm 250

output_filepath <- args[4] # <homedir>/<heatmapName>.txt

stats_method <- args[5] # method to use for stats

#message( paste("make_heatmap using grouping file", group_selection_filepath, " and stats method ", stats_method  ) )

# the script that will be run by the r session. Needs to be on the correct format to be read properly by the r source command. Change this line if you want to run your own heatmap function.
function_str <- paste("cellexalObj = make.cellexalvr.heatmap.list(cellexalObj,
 			\"", group_selection_filepath,
			"\",", top_genes_number,
			",\"", output_filepath,
			"\",\"", stats_method,
			"\")", sep="")

fileConn <- file(file.path(datadir, "mainServer.input.R"))
writeLines(function_str, fileConn)
close(fileConn)

