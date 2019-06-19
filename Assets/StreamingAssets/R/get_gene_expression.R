args <- commandArgs(trailingOnly = TRUE)

datadir <- args[1] # <data folder>

gene <- args[2]

function_str <- paste("write.table(t(cellexalObj@data[\"", gene, "\",]), file=\"", datadir, "/gene_expr.txt\", row.names=FALSE, col.names=TRUE, sep=\" \", quote=FALSE)", sep="")

#function_str <- paste("write.table(t(cellexalObj@data[\"", gene, "\", cellexalObj@data[\"", gene, "\",] > 0]), file=\"\", #row.names=TRUE, col.names=FALSE, sep=\" \", quote=FALSE)", sep="")


#function_str <- paste("cat(\"\nBEGIN\n\")","\nwrite.table(cellexalObj@data[\"", gene, "\",], file=\"\", row.names=TRUE, col.names=FALSE, sep=\" \", quote=FALSE)", "\ncat(\"\nEND\n\")", sep="") 

fileConn <- file(file.path(datadir, "geneServer.input.R"))
writeLines(function_str, fileConn)
close(fileConn)
