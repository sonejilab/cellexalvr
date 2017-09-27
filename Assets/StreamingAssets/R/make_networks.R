#print("start R")


library(cellexalvr)

args <- commandArgs(trailingOnly = TRUE)

homedir <- args[1]

datadir <- args[2]
latest_version <- args[3]
input_file_dir <- args[4]

expression_data_filepath <- file.path(datadir, "cellexalObj.RData")

group_selection_filepath <- file.path(input_file_dir, paste("selection", latest_version, ".txt", sep=""))

if ( ! file.exists(group_selection_filepath)) {
        group_selection_filepath = latest_version # as this is the grouping name!
}

#print(group_selection_filepath)

generated_table_filepath <- paste(file.path(homedir, "Resources","Networks"), "/", sep="")

cellexalvrObj <- make.cellexalvr.network(expression_data_filepath, group_selection_filepath, generated_table_filepath)

#save ( cellexalvrObj , file = file.path(args[4], "cellexalvrObj.RData", sep="" ))
