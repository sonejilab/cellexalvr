//----------------------------------------------
// SQLiter
// Copyright � 2014 OuijaPaw Games LLC
//----------------------------------------------

using UnityEngine;
using System.Data;
//using System.Collections.Generic;
using Mono.Data.SqliteClient;
using System.IO;
using System.Collections;
using System.Threading;


namespace SQLiter
{
    /// <summary>
    /// The idea is that here is a bunch of the basics on using SQLite
    /// Nothing is some advanced course on doing joins and unions and trying to make your infinitely normalized schema work
    /// SQLite is simple.  Very simple.  
    /// Pros:
    /// - Very simple to use
    /// - Very small memory footprint
    /// 
    /// Cons:
    /// - It is a flat file database.  You can change the settings to make it run completely in memory, which will make it even
    /// faster; however, you cannot have separate threads interact with it -ever-, so if you plan on using SQLite for any sort
    /// of multiplayer game and want different Unity instances to interact/read data... they absolutely cannot.
    /// - Doesn't offer as many bells and whistles as other DB systems
    /// - It is awfully slow.  I mean dreadfully slow.  I know "slow" is a relative term, but unless the DB is all in memory, every
    /// time you do a write/delete/update/replace, it has to write to a physical file - since SQLite is just a file based DB.
    /// If you ever do a write and then need to read it shortly after, like .5 to 1 second after... there's a chance it hasn't been
    /// updated yet... and this is local.  So, just make sure you use a coroutine or whatever to make sure data is written before
    /// using it.
    /// 
    /// SQLite is nice for small games, high scores, simple saved, etc.  It is not very secure and not very fast, but it's cheap,
    /// simple, and useful at times.
    /// 
    /// Here are some starting tools and information.  Go explore.
    /// </summary>
    public class SQLite : MonoBehaviour
    {
        public static SQLite Instance = null;
        public bool DebugMode = false;
        public StatusDisplay status;
        public bool QueryRunning { get; private set; }

        // Location of database - this will be set during Awake as to stop Unity 5.4 error regarding initialization before scene is set
        // file should show up in the Unity inspector after a few seconds of running it the first time
        private static string _sqlDBLocation = "";

        /// <summary>
        /// Table name and DB actual file name -- this is the name of the actual file on the filesystem
        /// </summary>
        //private const string SQL_DB_NAME = "Assets\\database.db";

        // table name
        private const string SQL_TABLE_NAME = "datavalues";
        /// <summary>
        /// DB objects
        /// </summary>
        private IDbConnection _connection = null;
        private IDbCommand _command = null;
        private IDataReader _reader = null;
        private string _sqlString;
        [HideInInspector]
        public ArrayList _result = new ArrayList();

        private bool _createNewTavle = false;

        /// <summary>
        /// Awake will initialize the connection.  
        /// RunAsyncInit is just for show.  You can do the normal SQLiteInit to ensure that it is
        /// initialized during the Awake() phase and everything is ready during the Start() phase
        /// </summary>
        void Awake()
        {
            if (DebugMode)
                Debug.Log("--- Awake ---");

            // here is where we set the file location
            // ------------ IMPORTANT ---------
            // - during builds, this is located in the project root - same level as Assets/Library/obj/ProjectSettings
            // - during runtime (Windows at least), this is located in the SAME directory as the executable
            // you can play around with the path if you like, but build-vs-run locations need to be taken into account
        }

        void Start()
        {
            if (DebugMode)
                Debug.Log("--- Start ---");

            // just for testing, comment/uncomment to play with it
            // note that it MUST be invoked after SQLite has initialized, 2-3 seconds later usually.  1 second is cutting it too close
            // Invoke("Test", 3);
        }

        /// <summary>
        /// Uncomment if you want to see the time it takes to do things
        /// </summary>
        //void Update()
        //{
        //    Debug.Log(Time.time);
        //}

        /// <summary>
        /// Clean up SQLite Connections, anything else
        /// </summary>
        void OnDestroy()
        {
            SQLiteClose();
        }

        /// <summary>
        /// Example using the Loom to run an asynchronous method on another thread so SQLite lookups
        /// do not block the main Unity thread
        /// </summary>
        public void RunAsyncInit()
        {
            LoomManager.Loom.QueueOnMainThread(() =>
            {
                SQLiteInit();
            });
        }

        public void InitDatabase(string path)
        {
            _sqlDBLocation = "URI=file:" + path;

            Debug.Log(_sqlDBLocation);
            Instance = this;
            SQLiteInit();
        }

        /// <summary>
        /// Basic initialization of SQLite
        /// </summary>
        private void SQLiteInit()
        {
            Debug.Log("SQLiter - Opening SQLite Connection");
            _connection = new SqliteConnection(_sqlDBLocation);
            _command = _connection.CreateCommand();
            _connection.Open();

            // WAL = write ahead logging, very huge speed increase
            _command.CommandText = "PRAGMA journal_mode = WAL;";
            _command.ExecuteNonQuery();

            // journal mode = look it up on google, I don't remember
            _command.CommandText = "PRAGMA journal_mode";
            _reader = _command.ExecuteReader();
            if (DebugMode && _reader.Read())
                Debug.Log("SQLiter - WAL value is: " + _reader.GetString(0));
            _reader.Close();

            // more speed increases
            _command.CommandText = "PRAGMA synchronous = OFF";
            _command.ExecuteNonQuery();

            // and some more
            _command.CommandText = "PRAGMA synchronous";
            _reader = _command.ExecuteReader();
            if (DebugMode && _reader.Read())
                Debug.Log("SQLiter - synchronous value is: " + _reader.GetInt32(0));
            _reader.Close();


            //_reader.GetSchemaTable();
            //// here we check if the table you want to use exists or not.  If it doesn't exist we create it.
            //_command.CommandText = "SELECT * FROM data LIMIT 1";
            //_reader = _command.ExecuteReader();
            //if (!_reader.Read())
            //{
            //    Debug.Log("SQLiter - Could not find SQLite table " + SQL_TABLE_NAME);
            //    // _createNewTavle = true;
            //}
            //_reader.Close();

            // create new table if it wasn't found
            //if (_createNewTavle)
            //{
            //    Debug.Log("SQLiter - Creating new SQLite table " + SQL_TABLE_NAME);

            //    // insurance policy, drop table
            //    _command.CommandText = "DROP TABLE IF EXISTS " + SQL_TABLE_NAME;
            //    _command.ExecuteNonQuery();

            //    // create new - SQLite recommendation is to drop table, not clear it
            //    _sqlString = "CREATE TABLE IF NOT EXISTS " + SQL_TABLE_NAME + " (" +
            //        COL_NAME + " TEXT UNIQUE, " +
            //        COL_RACE + " INTEGER, " +
            //        COL_CLASS + " INTEGER, " +
            //        COL_GOLD + " INTEGER, " +
            //        COL_LOGIN_LAST + " INTEGER, " +
            //        COL_LEVEL + " INTEGER, " +
            //        COL_XP + " INTEGER)";
            //    _command.CommandText = _sqlString;
            //    _command.ExecuteNonQuery();
            //}
            //else
            //{
            //    if (DebugMode)
            //        Debug.Log("SQLiter - SQLite table " + SQL_TABLE_NAME + " was found");
            //}

            // close connection
            _connection.Close();
        }

        #region Query
        /// <summary>
        /// Queries the database for the expressions of a gene.
        /// </summary>
        /// <param name="geneName"> The name of the gene </param>
        /// <returns> An array of all gene expression, ordered by cell </returns>
        public void QueryGene(string geneName)
        {
            QueryRunning = true;
            StartCoroutine(QueryGeneCoroutine(geneName));
        }

        /// <summary>
        /// Queries the database for the expression values for a gene and puts the result in _result
        /// </summary>
        /// <param name="geneName"> The gene name </param>
        private IEnumerator QueryGeneCoroutine(string geneName)
        {
            int statusId = status.AddStatus("Querying database for gene " + geneName);
            _result.Clear();
            string query = "select cname, value from datavalues left join cells on datavalues.cell_id = cells.id left join genes on datavalues.gene_id = genes.id where gname = \"" + geneName + "\"";
            Thread t = new Thread(() => QueryThread(query));
            t.Start();
            while (t.IsAlive)
            {
                yield return null;
            }
            int i = 0;
            float minExpr = float.MaxValue;
            float maxExpr = float.MinValue;
            while (_reader.Read())
            {
                float expr = _reader.GetFloat(1);
                if (expr > maxExpr)
                {
                    maxExpr = expr;
                }
                if (expr < minExpr)
                {
                    minExpr = expr;
                }
                i++;
                _result.Add(new CellExpressionPair(_reader.GetString(0), expr));
            }
            float binSize = (maxExpr - minExpr) / 30f;
            if (DebugMode)
            {
                print("binsize = " + binSize);
            }
            foreach (CellExpressionPair pair in _result)
            {
                pair.Expression = (pair.Expression - minExpr) / binSize;
            }
            if (DebugMode)
                print("Number of columns returned from database: " + i);
            _reader.Close();
            _connection.Close();
            status.RemoveStatus(statusId);
            QueryRunning = false;
        }

        /// <summary>
        /// Helper method that is run as a Thread.
        /// </summary>
        private void QueryThread(string query)
        {
            _connection.Open();
            _command.CommandText = query;
            _reader = _command.ExecuteReader();
        }
        #endregion

        /// <summary>
        /// Basic execute command - open, create command, execute, close
        /// </summary>
        /// <param name="commandText"></param>
        public void ExecuteNonQuery(string commandText)
        {
            _connection.Open();
            _command.CommandText = commandText;
            _command.ExecuteNonQuery();
            _connection.Close();
        }

        /// <summary>
        /// Clean up everything for SQLite
        /// </summary>
        private void SQLiteClose()
        {
            if (_reader != null && !_reader.IsClosed)
                _reader.Close();
            _reader = null;

            if (_command != null)
                _command.Dispose();
            _command = null;

            if (_connection != null && _connection.State != ConnectionState.Closed)
                _connection.Close();
            _connection = null;
        }
    }
    public class CellExpressionPair
    {
        public string Cell { get; private set; }
        public float Expression { get; set; }

        public CellExpressionPair(string Cell, float Expression)
        {
            this.Cell = Cell;
            this.Expression = Expression;
        }

    }
}
