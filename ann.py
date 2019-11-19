import anndata
import json
import sys
import numpy

if __name__ == "__main__":
    numpy.set_printoptions(threshold=sys.maxsize,linewidth=numpy.inf)
    file_name = sys.argv[1]
    #f = anndata.h5py.File("LCA_142K_umap_phate.h5ad",'r',rdcc_nbytes = (10242**2)*10)
    f = anndata.h5py.File(file_name,'r')
    while True:
        interp = input()
        try:
        	exec("print("+interp+")")
        except:
        	print("Something Went Wrong")
        sys.stdout.flush()
