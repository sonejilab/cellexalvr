library(gplots)
library(pheatmap)

anovap <- function(v,labs){
  anova(lm(v~-1+labs))$Pr[1]
}

make.heatmap.multigrp.anova <- function(dfile,cellidfile,num.sig,outfile){

  #dat.pre <- read.delim(dfile,row.names=NULL,header=T,check.names=F)
  load(dfile)
  dat <- as.matrix(gedata[,-1])
  rownames(dat) <- gedata[,1]

  cellid <- read.delim(cellidfile,header=F)
  #cellid.o <- cellid[order(cellid[,2]),]
  cellid.o <- cellid
  grp.vec <- as.vector(cellid.o[,2])
  #col.tab <- table(cellid.o[,2])
  col.tab <- unique(as.vector(cellid.o[,2]))

  for(i in 1:length(col.tab)){
    ind <- which(grp.vec==col.tab[i])
    grp.vec[ind] <- paste("Grp",i,sep="")
  }
  rcolrs <- list(Group=col.tab)
  names(rcolrs$Group) <- unique(grp.vec)
  #print(rcolrs)

  dat.s <- dat[,as.vector(cellid.o[,1])]
  #print(colnames(dat.s))
  rem.ind <- which(apply(dat.s,1,sum)==0)
  dat.f <- dat.s

  #print(rownames(dat.f)[1:10])

  if(length(rem.ind)>0){

    dat.f <- dat.s[-rem.ind,]
  }

  ps <- apply(dat.f,1,anovap,labs=grp.vec)

  sigp <- order(ps)[1:num.sig]

  #print(sort(ps)[1:20])

  annotation_col = data.frame(Group = (grp.vec))
  rownames(annotation_col) <- cellid.o[,1]
  #print(annotation_col)

  #print(dim(dat.f[sigp,]))

  png(outfile,height=800,width=1000)
  pheatmap(dat.f[sigp,],cluster_rows=TRUE, show_rownames=F,show_colnames=FALSE,cluster_cols=FALSE,scale="row",clustering_method = "ward.D2",col=bluered(16),breaks=seq(-4,4,by=0.5),annotation_col = annotation_col,annotation_colors=rcolrs)
  dev.off()
}

args <- commandArgs(trailingOnly = TRUE)
homedir <- args[1]
latest_version <- args[2]

expression_data_filepath <- paste(homedir, "/Assets/Scripts/R/gedata.RData", sep="")
group_selection_filepath <- paste(homedir, "/Assets/Data/runtimeGroups/selection", latest_version, ".txt", sep="")
generated_image_filepath <- paste(homedir, "/Assets/Images/heatmap.png", sep="")
top_genes_number <- 500
make.heatmap.multigrp.anova(expression_data_filepath,group_selection_filepath,top_genes_number,generated_image_filepath)
