import h5py
import json
import sys
import numpy

if __name__ == "__main__":
    numpy.set_printoptions(threshold=sys.maxsize,linewidth=numpy.inf)
    file = h5py.File("LCA_142K_umap_phate.h5ad",'r')
    while True:
        interp = raw_input()
        exec("print("+interp+")")
        sys.stdout.flush()
