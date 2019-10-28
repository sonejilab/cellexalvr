#print("start R")

args <- commandArgs(trailingOnly = TRUE)

datadir <- args[1] # the user specific folder

group_selection_filepath <- args[2] # grouping file path

output_filepath  <- args[3] # the output path

network_method <- args[4] # the algorithm to use

expression_data_filepath <- file.path(datadir, "cellexalObj.RData")

# the script that will be run by the r session. Needs to be on the correct format to be read properly by the r source command. Change this line if you want to run your own network function.
function_str <- paste("make.cellexalvr.network(cellexalObj,
 			\"", group_selection_filepath, "\",
			\"", output_filepath, "\",
			\"method=", network_method , "\")",
			sep="")

fileConn <- file(file.path(datadir, "mainServer.input.R"))
writeLines(function_str, fileConn)
close(fileConn)
