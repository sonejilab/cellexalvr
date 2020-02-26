//using UnityEngine;
//using System.Collections;
//using AS.HDFql;
//using System.IO;

//public class HDF5Reader : MonoBehaviour
//{
//    private string path;
//    // Use this for initialization
//    void Start()
//    {
//        path = "Data/Gastrulation/test.h5";
//        print(File.Exists(path));
//        HDFql.Execute("USE FILE " + path);

//        HDFql.Execute("SHOW USE FILE");

//        HDFql.CursorFirst();
//        print("File in use: " + HDFql.CursorGetChar());

//        HDFql.Execute("SHOW DIMENSION group/data");
//        HDFql.CursorFirst();
//        int length = (int)HDFql.CursorGetInt();

//        print(length);

//        int[] indices = new int[length];
//        float[] data = new float[length];
//        HDFql.VariableRegister(indices);

//        HDFql.Execute("SELECT FROM group/indptr INTO MEMORY " + HDFql.VariableGetNumber(indices));


//        //HDFql.VariableRegister(data);

//        //HDFql.Execute("SELECT FROM group/data INTO MEMORY " + HDFql.VariableGetNumber(data));
//        //print(data.Length);
//        //for (int i = 0; i < 10; i++)
//        //{
//        //    print(data[i]);
//        //}
//    }



//    // Update is called once per frame
//    void Update()
//    {

//    }
//}
