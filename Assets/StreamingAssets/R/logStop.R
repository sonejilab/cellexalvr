args <- commandArgs(trailingOnly = TRUE)

datadir <- args[1] ## <user specific folder>

function_str <- paste("renderReport(cellexalObj)")

fileConn <- file(file.path(datadir, "server.input.R"))
writeLines(function_str, fileConn)
close(fileConn)
