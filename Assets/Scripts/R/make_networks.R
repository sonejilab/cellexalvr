#print("start R")


library(cellexalvr)

args <- commandArgs(trailingOnly = TRUE)

homedir <- args[1]

datadir <- args[2]
latest_version <- args[3]


expression_data_filepath <- file.path(datadir, "cellexalObj.RData")

group_selection_filepath <- file.path(homedir, "Data","runtimeGroups", paste("selection", latest_version, ".txt", sep=""))

#print(group_selection_filepath)

generated_table_filepath <- paste(file.path(homedir, "Resources","Networks"), "/", sep="")

make.cellexalvr.network(expression_data_filepath, group_selection_filepath, generated_table_filepath)