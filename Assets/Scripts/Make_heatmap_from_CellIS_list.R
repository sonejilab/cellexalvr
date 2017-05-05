print("script started")
library(gplots)
library(pheatmap)
print("libs loaded")

make.heatmap.1grp <- function(dfile,cellidfile,outfile){
  
  print("func started")
  dat.pre <- read.delim(dfile,row.names=NULL,header=T,check.names=F)
  print("data loaded")
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

  print(outfile)
  png(outfile,height=800,width=1000)
  print("created png")
  pheatmap(dat.f,cluster_rows=TRUE, show_rownames=FALSE,show_colnames=FALSE,cluster_cols=TRUE,scale="row",clustering_method = "ward.D2",col=bluered(16),breaks=seq(-4,4,by=0.5))
  dev.off()
  print("almost done")
}

print(getwd())
setwd("C:/Users/Kristian/Documents/Unity_projects/vrJeans/Assets/Images")
print(getwd())
expression_data_filepath <- "C:/Users/Kristian/Documents/Unity_projects/vrJeans/Assets/Data/Gene_Expression_Data.txt"
group_selection_filepath <- "C:/Users/Kristian/Documents/Unity_projects/vrJeans/Assets/Data/CellIDs2heatmap_1GRP.txt"
make.heatmap.1grp(expression_data_filepath, group_selection_filepath,"heatmap.png")
print("done")