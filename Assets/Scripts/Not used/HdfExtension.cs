//using HDF5DotNet;
//using System;
//using System.Collections.Generic;
//using System.Linq;

///// <summary>
///// This class is a helper class to read hdf5 files.
///// Thanks stackoverflow :)
///// </summary>
//public static class HdfExtensions
//{
//    // thank you http://stackoverflow.com/questions/4133377/splitting-a-string-number-every-nth-character-number
//    public static IEnumerable<String> SplitInParts(this String s, Int32 partLength)
//    {
//        if (s == null)
//            throw new ArgumentNullException("s");
//        if (partLength <= 0)
//            throw new ArgumentException("Part length has to be positive.", "partLength");

//        for (var i = 0; i < s.Length; i += partLength)
//            yield return s.Substring(i, Math.Min(partLength, s.Length - i));
//    }



//    public static T[] Read1DArray<T>(this H5FileId fileId, string dataSetName)
//    {
//        var dataset = H5D.open(fileId, dataSetName);
//        var space = H5D.getSpace(dataset);
//        var dims = H5S.getSimpleExtentDims(space);
//        var dataType = H5D.getType(dataset);
//        if (typeof(T) == typeof(string))
//        {
//            int stringLength = H5T.getSize(dataType);
//            byte[] buffer = new byte[dims[0] * stringLength];
//            H5D.read(dataset, dataType, new H5Array<byte>(buffer));
//            string stuff = System.Text.ASCIIEncoding.ASCII.GetString(buffer);
//            return stuff.SplitInParts(stringLength).Select(ss => (T)(object)ss).ToArray();
//        }
//        T[] dataArray = new T[dims[0]];
//        var wrapArray = new H5Array<T>(dataArray);
//        H5D.read(dataset, dataType, wrapArray);
//        return dataArray;
//    }

//    public static T[,] Read2DArray<T>(this H5FileId fileId, string dataSetName)
//    {
//        var dataset = H5D.open(fileId, dataSetName);
//        var space = H5D.getSpace(dataset);
//        var dims = H5S.getSimpleExtentDims(space);
//        var dataType = H5D.getType(dataset);
//        if (typeof(T) == typeof(string))
//        {
//            // this will also need a string hack...
//        }
//        T[,] dataArray = new T[dims[0], dims[1]];
//        var wrapArray = new H5Array<T>(dataArray);
//        H5D.read(dataset, dataType, wrapArray);
//        return dataArray;
//    }
//}