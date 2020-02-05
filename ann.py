import anndata
import sys
import numpy as np
import six

def numpystr_fmt(x, encoding='latin1'):
    if isinstance(x, np.str_):
        out = x
    elif six.PY3:
        out = x.decode(encoding) ##np.bytes_
    elif isinstance(x, np.unicode_):
        out = x.encode(encoding)
    return "%r" %out

np.set_printoptions(threshold=sys.maxsize,linewidth=np.inf, formatter={'str_kind': numpystr_fmt})
file_name = sys.argv[1]
f = anndata.h5py.File(file_name,'r')
while True:
    interp = input()
    try:
        exec("print("+interp+")")
    	#exec("print(np.array2string("+interp+", separator=','))")
    except Exception as e:
    	print(e)
    sys.stdout.flush()
