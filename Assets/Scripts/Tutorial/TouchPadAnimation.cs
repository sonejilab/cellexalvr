using UnityEngine;
using System.Collections;
using VRTK.Examples;

namespace CellexalVR.Tutorial
{
    public class TouchPadAnimation : MonoBehaviour
    {
        public Vector3 targetPos;
        public TouchPadSequence touchPadSequence;
        public int seqNr;

        private bool animate;
        private float speed = 3.2f;
        private float growSpeed = 0.7f;
        // Use this for initialization
        void Start()
        {
            animate = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (animate)
            {
                float step = speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
                transform.localScale += Vector3.one * Time.deltaTime * growSpeed;
                transform.Rotate(0, 0, 250 * Time.deltaTime);
                if (transform.localScale.magnitude > Vector3.one.magnitude)
                {
                    animate = false;
                    transform.localRotation = Quaternion.identity;
                    touchPadSequence.UpdateSequence(GetComponent<Renderer>().material.color, seqNr);
                    Destroy(gameObject);

                }
            }
        }

        // Animation when initiating object.
        void Animate()
        {

        }
    }
}
