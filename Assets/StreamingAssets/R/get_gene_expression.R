args <- commandArgs(trailingOnly = TRUE)

datadir <- args[1] # <data folder>

gene <- args[2]

function_str <- paste("write.table(cellexalObj@data[\"", gene, "\", cellexalObj@data[\"", gene, "\",] > 0], file=\"", datadir, "/gene_expr.txt\", row.names=TRUE, col.names=FALSE, sep=\" \", quote=FALSE)", sep="") 
fileConn <- file(file.path(datadir, "server.input.R"))
writeLines(function_str, fileConn)
close(fileConn)
