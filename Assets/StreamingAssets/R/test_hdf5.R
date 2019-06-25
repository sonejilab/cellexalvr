library(cellexalvrR)

cellexalObj <- loadObject("../../../Data/Gastrulation/cellexalObj.RData")

# library(DropletUtils)

# write10xCounts('test.h5', cellexalObj@data, overwrite=TRUE, type="HDF5")

library(rhdf5)

hf <- H5Fopen('../../../Data/Gastrulation/testing.h5')

# gene = "Gata1"

# gene_ind <- which(hf$group$genes == gene)

# indices <- which(hf$group$indices == gene_ind)

# x <- rbind(c(indices), c(hf$group$data[indices]))

# write.table(x, file="test.txt", sep=",", col.names=FALSE, row.names=FALSE)



# h5createFile("test2.h5")
# h5createGroup('test2.h5', "gene_name")
# i = 0
# for (gene in hf$group$genes) {
#     indices <- which(hf$group$indices == i)
#     # print(paste("Gene", i, "of", length(hf$group$genes), sep=" "))
#     h5write(indices, "test2.h5", paste("gene_name", gene, sep="/"))

#     i = i + 1
# }

#h5createGroup("test2.h5", gene)

h5createFile("../../../Data/Gastrulation/mydata.h5")
h5createGroup("../../../Data/Gastrulation/mydata.h5", "expressions")
h5createGroup("../../../Data/Gastrulation/mydata.h5", "names")

i = 0
for (gene in hf$group$genes) {
    gene_ind <- which(hf$group$genes == gene)
    indices <- which(hf$group$indices == gene_ind)

    expressions <- hf$group$data[indices]

    # expr_slot <- cellexalObj@data[gene, ]
    print(paste(i, "of", length(rownames(cellexalObj@data)), sep=" "))
    # if (length(expr_slot) > 0) {
    #     hdf5_expr_slot <- paste("expressions", gene, sep="/")
    #     h5write(expr_slot, file="../../../Data/Gastrulation/mydata.h5", hdf5_expr_slot)
    #     # name_slot <- names(expr_slot)
    #     # if (length(name_slot) > 0) {
    #     #     hdf5_name_slot <- paste("names", gene, sep="/")

    #     #     h5write(name_slot, file="../../../Data/Gastrulation/mydata.h5", hdf5_name_slot)
    #     # }

    # }
    i = i + 1
}

H5close()