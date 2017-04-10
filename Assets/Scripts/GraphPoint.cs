using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphPoint : MonoBehaviour
{
		public GameObject prefab;
		private Cell cell;
		private float x, y, z;

	public void setCoordinates (Cell cell, float x, float y, float z, Vector3 graphAreaSize)
	{
		this.cell = cell;
		this.x = x;
		this.y = y;
		this.z = z;
		GameObject sphere = Instantiate (prefab, new Vector3 (x, y, z), Quaternion.identity);
		/**
			 * TODO: scale the sphere dependent on the graphArea.
			 **/
	}
}

