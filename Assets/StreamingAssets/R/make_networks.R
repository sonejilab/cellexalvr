#print("start R")


suppressMessages(library(cellexalvrR))

args <- commandArgs(trailingOnly = TRUE)

input_file <- args[1] # grouping file path

datadir <- args[2] # the user specific folder

output_file  <- args[3] # the output path

expression_data_filepath <- file.path(datadir, "cellexalObj.RData")

message( paste("make_network using grouping file", input_file ))

cellexalvrObj <- make.cellexalvr.network(expression_data_filepath, input_file,  output_file)

message( "Save updated cellexalvrR object" )

lockedSave(cellexalObj, path=datadir )

