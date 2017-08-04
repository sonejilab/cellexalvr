using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using BayatGames.SaveGameFree.Types;
using BayatGames.SaveGameFree.Serializers;

namespace BayatGames.SaveGameFree.Examples
{

	public class SaveScene : MonoBehaviour
	{
		//public SaveableData saveableData;
		public string PosIdentifier1 = "SaveGraphPos1.dat";
		public string PosIdentifier2 = "SaveGraphPos2.dat";
		public string RotIdentifier1 = "SaveGraphRot1.dat";
		public string RotIdentifier2 = "SaveGraphRot2.dat";
		public string DataIdentifier = "SaveData.dat";
		public string targetDir;
		public Transform target1;
		public Transform target2;
		public GraphManager graphManager;
		public bool loadOnStart = true;
		public Graph graphPrefab;
		public SaveGameXmlSerializer serializer = new SaveGameXmlSerializer ();

		void Start()
		{

			if ( loadOnStart )
			{
				LoadPositions ();
				LoadDirectory ();
			}
		}



		void Update ()
		{
			if (target1 == null) {
				Graph[] graphs = graphManager.GetComponentsInChildren<Graph> ();
				if (graphs.Length > 0) {
					target1 = graphs [0].transform;
				}
			}
			if (target2 == null) {
				Graph[] graphs = graphManager.GetComponentsInChildren<Graph> ();
				if (graphs.Length > 1) {
					target2 = graphs [1].transform;
				}
			}
			/*
			if (target1 == null && target2 == null)
				return;
			Vector3 newPosition1 = target1.position;
			newPosition1.x += Input.GetAxis ( "Horizontal" );
			newPosition1.y += Input.GetAxis ( "Vertical" );
			target1.position = newPosition1;

			Vector3 rotation1 = target1.rotation.eulerAngles;
			rotation1.z += Input.GetAxis ( "Horizontal" );
			target1.rotation = Quaternion.Euler ( rotation1 );

			Vector3 newPosition2 = target2.position;
			newPosition2.x += Input.GetAxis ( "Horizontal" );
			newPosition2.y += Input.GetAxis ( "Vertical" );
			target2.position = newPosition2;

			Vector3 rotation2 = target2.rotation.eulerAngles;
			rotation2.z += Input.GetAxis ( "Horizontal" );
			target2.rotation = Quaternion.Euler ( rotation2 );
			*/
		}


		void OnApplicationQuit ()
		{
			
		} 

		public void Save ()
		{
			SaveGame.Save<Vector3Save> ( PosIdentifier1, target1.position, serializer );
			Debug.Log ("Save Pos 1: " + target1.position);
			SaveGame.Save<Vector3Save> ( PosIdentifier2, target2.position, serializer );
			Debug.Log ("Save Pos 2: " + target2.position);
			//SaveGame.Save<Vector3Save> (identifier, target2.position, serializer);
			SaveGame.Save<QuaternionSave> ( RotIdentifier1, target1.rotation, serializer );
			Debug.Log ("Save Rot 1: " + target1.position);
			SaveGame.Save<QuaternionSave> ( RotIdentifier2, target2.rotation, serializer );
			Debug.Log ("Save Rot 2: " + target2.position);
			SaveGame.Save<string> (DataIdentifier, graphManager.directory, serializer);
			Debug.Log ("Save Dir: " + graphManager.directory);

			Debug.Log ("Scene Saved");
		}

		public void LoadPositions ()
		{
			Debug.Log ("LOADING POSITIONS");
			target1.position = SaveGame.Load<Vector3Save> (
				PosIdentifier1,
				Vector3.zero,
				serializer);
			Debug.Log ("Load Pos1: " + target1.position);
			target2.position = SaveGame.Load<Vector3Save> (
				PosIdentifier2,
				Vector3.zero,
				serializer);
			Debug.Log ("Load Pos2: " + target2.position);

			target1.rotation = SaveGame.Load<QuaternionSave> (
				RotIdentifier1,
				Quaternion.identity,
				serializer );
			Debug.Log ("Load Rot1:  " + target1.rotation);

			target2.rotation = SaveGame.Load<QuaternionSave> (
				RotIdentifier2,
				Quaternion.identity,
				serializer );
			Debug.Log ("Load Rot2:  " + target2.rotation);
		}

		public void LoadDirectory ()
		{
			targetDir = SaveGame.Load<string> (DataIdentifier,
				"", 
				serializer);
			Debug.Log ("Load dir: " + targetDir);
		}

		public void SetGraph (Graph graph, int graphNr)
		{
			if (graphNr == 1) {
				target1 = graph.transform;
				target2 = graph.transform;
			} else {
				target2 = graph.transform;
			}
		}
	





	}

}