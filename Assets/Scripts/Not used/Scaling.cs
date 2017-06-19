// Base Grab Action|SecondaryControllerGrabActions|60010
namespace VRTK.SecondaryControllerGrabActions
{
using UnityEngine;

/// <summary>
/// The Base Grab Action is an abstract class that all other secondary controller actions inherit and are required to implement the public abstract methods.
/// </summary>
/// <remarks>
/// As this is an abstract class, it cannot be applied directly to a game object and performs no logic.
/// </remarks>
public class Scaling : VRTK_BaseGrabAction
{
public GameObject leftController;
public GameObject rightController;
private bool jointed;
private float startingDist;
private Vector3 startingScale;

void ProcessUpdate() {
	float distance = Vector3.Distance (leftController.transform.position, rightController.transform.position);
	startingScale *= distance / startingDist;
	grabbedObject.GetComponent<Graph> ().transform.localScale = startingScale;

}

}

}
