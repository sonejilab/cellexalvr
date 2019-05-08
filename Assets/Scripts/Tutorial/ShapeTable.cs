using UnityEngine;
using System.Collections.Generic;


namespace CellexalVR.Tutorial
{
    public class ShapeTable : MonoBehaviour
    {
        private IntroTutorialManager introManager;

        private Dictionary<string, bool> shapes;
        void Start()
        {
            introManager = GameObject.Find("IntroTutorialManager").GetComponent<IntroTutorialManager>();
            shapes = new Dictionary<string, bool>
            {
                ["Cube"] = false,
                ["Triangle"] = false,
                ["Sphere"] = false,
                ["Torus"] = false,

            };
        }

        private void OnTriggerEnter(Collider other)
        {
            shapes[other.name] = true;
            if (!shapes.ContainsValue(false))
            {
                // all shapes are in, we are done.
                print("All done");
                Destroy(gameObject);
                introManager.TouchPadLevel();
            }
        }
    }
}
