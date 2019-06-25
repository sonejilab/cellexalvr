args <- commandArgs(trailingOnly = TRUE)

datadir <- args[1]

gene_name <- args[2]

output_filepath <- args[3]

facsTypeArg <- as.logical(args[4])

expression_data_filepath <- file.path(datadir, "cellexalObj.RData")


# the script that will be run by the r session. Needs to be on the correct format to be read properly by the r source command. Change this line if you want to run your own correlated genes function.

function_str <- paste("get.genes.cor.to(cellexalObj,
			\"", gene_name, "\",
			\"", output_filepath, "\",
			\"", facsTypeArg, "\")", 
			sep="")


print(function_str)

fileConn <- file(file.path(datadir, "mainServer.input.R"))
writeLines(function_str, fileConn)
close(fileConn)
