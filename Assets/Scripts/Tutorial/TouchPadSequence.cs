using UnityEngine;
using CellexalVR.General;
using System.Collections.Generic;
using System.Collections;

namespace CellexalVR.Tutorial
{
    public class TouchPadSequence : MonoBehaviour
    {
        public GameObject[] objs = new GameObject[5];
        public ReferenceManager referenceManager;
        public GameObject squarePrefab;

        private Dictionary<int, Color> sequence = new Dictionary<int, Color>();
        private Color[] colors = new Color[4];
        private int seqCtr;
        private int squareNr;
        private IntroTutorialManager introManager;
        private GameObject square;
        private Vector3[] positions;
        private List<Color> orgColors = new List<Color>();
        private bool endingAnimation;
        private float time = 0f;
        private int k = 0;

        void Start()
        {
            colors[0] = Color.red;
            colors[1] = Color.green;
            colors[2] = Color.blue;
            colors[3] = Color.yellow;

            sequence[0] = colors[0]; // red
            sequence[1] = colors[2]; // blue
            sequence[2] = colors[1]; // green
            sequence[3] = colors[0]; // red
            sequence[4] = colors[3]; // yellow
            seqCtr = 0;
            introManager = GameObject.Find("IntroTutorialManager").GetComponent<IntroTutorialManager>();

        }

        private void Update()
        {
            if (endingAnimation)
            {
                time += Time.deltaTime;
                if (time > 3.5f)
                {
                    endingAnimation = false;
                    introManager.Final();
                    Destroy(gameObject);
                }
                if (time > 1.0f)
                {
                    k = 1;
                }
                if (time > 1.5f)
                {
                    k = 2;
                }
                if (time > 2.0f)
                {
                    k = 3;
                }
                if (time > 2.5f)
                {
                    k = 4;
                }
                for (int i = 0; i <= k; i++)
                {
                    objs[i].transform.Translate(0, time / 30, 0);
                }

            }
        }

        public void HandleButtonClick(int button)
        {
            if (squareNr < 5)
            {
                Vector3 startPos;
                if (button > 3)
                {
                    startPos = referenceManager.rightController.transform.position;
                }
                else
                {
                    startPos = referenceManager.leftController.transform.position;
                }
                TouchPadAnimation square = Instantiate(squarePrefab, startPos, Quaternion.identity, transform).GetComponent<TouchPadAnimation>();
                square.touchPadSequence = this;
                //square.seqNr = squareNr;
                square.targetPos = objs[squareNr].transform.position;
                square.GetComponent<Renderer>().material.color = colors[button % colors.Length];
                squareNr++;
            }
        }

        public void UpdateSequence(Color color, int nr)
        {
            if (sequence[seqCtr].Equals(color))
            {
                //objs[pressCtr].SetActive(true);
                orgColors.Add(objs[seqCtr].GetComponent<Renderer>().material.color);
                objs[seqCtr].GetComponent<Renderer>().material.color = color;
                seqCtr++;
            }
            else
            {
                for (int i = 0; i < orgColors.Count; i++)
                {
                    objs[i].GetComponent<Renderer>().material.color = orgColors[i];
                    //orgColors.RemoveAt(i);
                }
                orgColors.Clear();
                seqCtr = 0;
                squareNr = 0;
            }
            if (seqCtr == 5)
            {
                endingAnimation = true;
                //introManager.Final();
            }

        }
        
    }
}
