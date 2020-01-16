#!/usr/bin/env Rscript

library(loomR)
library(cellexalvrR)

args = commandArgs(TRUE)

if(is.na(args[1]))
	args[1] <- "Data/Oregano"

path = args[1]

datafile = list.files(path, pattern = "\\.loom$|\\.h5ad$")[1]

configFilePath = list.files(path, pattern = "\\.conf$")[1]


configFile <- readLines(paste(path, configFilePath, sep="\\"))
conf = list()

for(c in configFile){
	cc = regmatches(c, regexpr(" ", c), invert = TRUE)
	conf[[cc[[1]][1]]] <- cc[[1]][2]
}
print(as.logical(conf$gene_x_cell))
print(conf)
f <- connect(paste(path, datafile, sep="\\"), mode = 'r') 
dims = f[[conf$cellexpr]]$dims
n = dims[1]
m = dims[2]

if(as.logical(conf$gene_x_cell)){
	exdata = t(f[[conf$cellexpr]][1:n,1:m])
}else{
	exdata = f[[conf$cellexpr]][1:n,1:m]
}

rown = f[[conf$genenames]][1:m]
coln = f[[conf$cellnames]][1:n]

colnames(exdata) <- coln
rownames(exdata) <- rown

proj.list = list()
 	
if(is.null(conf[["2D_sep"]]) || !as.logical(conf[["2D_sep"]])){
	for(x in names(conf)){
		if(startsWith(x, 'X')){
			name = strsplit(x, '_')[[1]][2]
			proj.list[[name]] = t(f[[conf[[x]]]][1:3,1:n])
		}
	}
}else{	
	proj.list[["sep"]] = cbind(f[[conf$X_sep]][1:n], f[[conf$Y_sep]][1:n], 0)
}

cellexalObj <- MakeCellexaVRObj(Matrix::Matrix(exdata, sparse=T), drc.list=proj.list, specie="mouse")
if(TRUE){
	save.image(file = paste(path,"/cellexalObj.RData", sep=""))  
}