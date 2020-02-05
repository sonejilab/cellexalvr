#!/usr/bin/env Rscript

library(Seurat)
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


#For h5ad files
if(endsWith(datafile, ".h5ad")){


	bm<- ReadH5AD(paste(path, datafile, sep="\\"))
	x <- hdf5r::h5file(paste(path, datafile, sep="\\"))
	#arr.len <- 2
	#scl <- 1/arr.len
	umap <- t(as.matrix(x[['obsm']][['X_umap']][,]))
	phate <- t(as.matrix(x[['obsm']][['X_phate']][,]))
	pc <- phate
	pc[,1] <- round(pc[,1],4)+sample(c(-0.0003,-0.0002,-0.0001,0,0.0001,0.0002,0.0003),141723,replace=T)
	pc[,2] <- round(pc[,2],4)+sample(c(-0.0003,-0.0002,-0.0001,0,0.0001,0.0002,0.0003),141723,replace=T)
	pc[,3] <- round(pc[,3],4)+sample(c(-0.0003,-0.0002,-0.0001,0,0.0001,0.0002,0.0003),141723,replace=T)
	c1 <- which(duplicated(pc[,1])==T)
	c2 <- which(duplicated(pc[,2])==T)
	c3 <- which(duplicated(pc[,3])==T)
	length(intersect(intersect(c1,c2),c3))
	phate[ which(duplicated(phate[,1])==T),] <- rnorm( length(which((duplicated(phate[,1]))==T))*3   ,sd=0.014) #make them unique
	#vel.umap <- t(  as.matrix(x[['obsm']][['X_umap']][,])+ 30*(as.matrix(x[['obsm']][['velocity_umap']][,])))
	#vel.phate <- t(as.matrix(x[['obsm']][['X_phate']][,])+ 30*(as.matrix(x[['obsm']][['velocity_phate']][,])))
	#vel.phate <- t(as.matrix(x[['obsm']][['X_phate']][,])+ 30*(as.matrix(x[['obsm']][['velocity_phate']][,])))
	vel.umap <- umap+ t(30*(as.matrix(x[['obsm']][['velocity_umap']][,])))
	vel.phate <- phate+t(30*(as.matrix(x[['obsm']][['velocity_phate']][,])))
	#vel.sc <- t(as.matrix(x[['obsm']][['X_umap']][,])+  ((as.matrix(x[['obsm']][['velocity_umap']][,]))/scl/5))
	rownames(umap) <- colnames(bm)
	rownames(phate) <- colnames(bm)
	rownames(vel.umap) <- colnames(bm)
	rownames(vel.phate) <- colnames(bm)
	vel.umap.out <- cbind(umap,vel.umap)
	vel.phate.out <- cbind(phate,vel.phate)
	#vel.phate.out.com <- vel.phate.out/10

	###### Test phate dups
	rownames(phate) <- colnames(bm)
	bm[["umap"]] <- CreateDimReducObject(embeddings = umap,key="UMAP_", assay = DefaultAssay(bm))
	bm[["phate"]] <- CreateDimReducObject(embeddings =vel.phate.out[,1:3],key="PHATE_", assay = DefaultAssay(bm))
	rownames(vel.phate.out) <- colnames(bm)
	colnames(vel.phate.out) <- c("dim1","dim2",	"dim3",	"velo1","velo2","velo3")
	vel.phate.nodup <- vel.phate.out[-which(duplicated(vel.phate.out[,1])==T),]
	#FeaturePlot(bm,"CD34")
	#DimPlot(bm)
	ndr <- list(UMAP=Embeddings(bm,"umap"),PHATE=Embeddings(bm,"phate"))
	bm.data <- GetAssayData(object = bm)
	bm.meta <- make.cell.meta.from.df(bm[[]],"louvain")
	cellexalObj <- new("cellexalvrR",data=bm.data,drc=ndr)
	cellexalObj <- set.specie(cellexalObj,"human") # Set the specie to Mouse
	cellexalObj <- addCellMeta2cellexalvr(cellexalObj,bm.meta)
	cellexalObj <- addVelocityToExistingDR(cellexalObj,vel.umap.out,"UMAP")
	cellexalObj <- addVelocityToExistingDR(cellexalObj,vel.phate.out,"PHATE")
	ofile = file.path( path, "cellexalObj.RData")
	if ( ! file.exists( ofile) ){
		save(cellexalObj,file=ofile )
	}
    


}else{

	f <- connect(paste(path, datafile, sep="\\"), mode = 'r', skip.validate = TRUE) 
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
	ofile = file.path( path, "cellexalObj.RData")
	if ( !file.exists( ofile) ){
		save(cellexalObj,file=ofile )
	}
}
