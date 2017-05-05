library(gplots)
library(pheatmap)

make.heatmap.1grp <- function(dfile,cellidfile,outfile){

  dat.pre <- read.delim(dfile,row.names=NULL,header=T,check.names=F)
  dat <- as.matrix(dat.pre[,-1])
  rownames(dat) <- dat.pre[,1]

  cellid <- readLines(cellidfile)

  dat.s <- dat[sample(1:4500,800) ,cellid]
  rem.ind <- which(apply(dat.s,1,sum)==0)
  dat.f <- dat.s

  if(length(rem.ind)>0){

    dat.f <- dat.s[-rem.ind,]
  }

  print(dim(dat.s))
  print(dim(dat.f))

  #heatmap.2(dat.s,trace="none",scale="row",col=bluered(30))

  png(outfile,height=800,width=1000)
  pheatmap(dat.f,cluster_rows=TRUE, show_rownames=FALSE,show_colnames=FALSE,cluster_cols=TRUE,scale="row",clustering_method = "ward.D2",col=bluered(16),breaks=seq(-4,4,by=0.5))
  dev.off()
}


make.heatmap.1grp("Gene_Expression_Data.txt","CellIDs2heatmap_1GRP.txt","heatmap_out.png")
