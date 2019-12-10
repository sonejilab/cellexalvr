import anndata
import sys
import numpy as np

if __name__ == "__main__":
    np.set_printoptions(threshold=sys.maxsize,linewidth=np.inf)
    file_name = sys.argv[1]
    f = anndata.h5py.File(file_name,'r')
    while True:
        interp = input()
        try:
        	exec("print("+interp+")")
        except:
        	print("Something Went Wrong")
        sys.stdout.flush()
