//using HDF5DotNet;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using UnityEngine;

//public class HDFReader : MonoBehaviour
//{
//    public Array Read1DArray(H5FileId fileId, string dataSetName)
//    {
//        var dataset = H5D.open(fileId, dataSetName);
//        var space = H5D.getSpace(dataset);
//        var dims = H5S.getSimpleExtentDims(space);
//        var dtype = H5D.getType(dataset);

//        var size = H5T.getSize(dtype);
//        var classID = H5T.getClass(dtype);

//        var rank = H5S.getSimpleExtentNDims(space);
//        var status = H5S.getSimpleExtentDims(space);

//        // Read data into byte array
//        var dataArray = new Byte[status[0] * size];
//        var wrapArray = new H5Array<Byte>(dataArray);
//        H5D.read(dataset, dtype, wrapArray);

//        // Convert types
//        Array returnArray = null;
//        Type dataType = null;

//        switch (classID)
//        {
//            case H5T.H5TClass.STRING:
//                dataType = typeof(string);
//                break;

//            case H5T.H5TClass.FLOAT:
//                if (size == 4)
//                    dataType = typeof(float);
//                else if (size == 8)
//                    dataType = typeof(double);
//                break;

//            case H5T.H5TClass.INTEGER:
//                if (size == 2)
//                    dataType = typeof(Int16);
//                else if (size == 4)
//                    dataType = typeof(Int32);
//                else if (size == 8)
//                    dataType = typeof(Int64);
//                break;

//        }

//        if (dataType == typeof(string))
//        {
//            var cSet = H5T.get_cset(dtype);

//            string[] stringArray = new String[status[0]];

//            for (int i = 0; i < status[0]; i++)
//            {
//                byte[] buffer = new byte[size];
//                Array.Copy(dataArray, i * size, buffer, 0, size);

//                Encoding enc = null;
//                switch (cSet)
//                {
//                    case H5T.CharSet.ASCII:
//                        enc = new ASCIIEncoding();
//                        break;
//                    case H5T.CharSet.UTF8:
//                        enc = new UTF8Encoding();
//                        break;
//                    case H5T.CharSet.ERROR:
//                        break;
//                }

//                stringArray[i] = enc.GetString(buffer).TrimEnd('\0');
//            }

//            returnArray = stringArray;
//        }
//        else
//        {
//            returnArray = Array.CreateInstance(dataType, status[0]);
//            Buffer.BlockCopy(dataArray, 0, returnArray, 0, (int)status[0] * size);
//        }

//        H5S.close(space);
//        H5T.close(dtype);
//        H5D.close(dataset);

//        return returnArray;
//    }
//}
