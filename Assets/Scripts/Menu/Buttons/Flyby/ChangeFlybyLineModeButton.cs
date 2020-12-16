using CellexalVR.Interaction;
using CellexalVR.Menu.SubMenus;
using UnityEngine;

namespace CellexalVR.Menu.Buttons.Flyby
{

    /// <summary>
    /// The button between two positions that the flyby camera flies between.
    /// </summary>
    public class ChangeFlybyLineModeButton : CellexalButton
    {
        public int Index { get; set; }
        public LineRenderer lineRenderer;

        public FlybyMenu.FlybyLineMode mode = FlybyMenu.FlybyLineMode.Linear;

        private FlybyMenu flybyMenu;
        private InteractableObjectBasic interactableObject;

        protected override string Description => "Click to change line mode";

        private void Start()
        {
            flybyMenu = referenceManager.flybyMenu;
        }

        public override void Click()
        {
            bool switchToLinear = mode == FlybyMenu.FlybyLineMode.Bezier;
            if (switchToLinear)
            {
                mode = FlybyMenu.FlybyLineMode.Linear;
                transform.position = flybyMenu.HalfwayPosition(Index);
                meshStandardColor = new Color(0.5f, 0.5f, 0.5f);
                meshHighlightColor = new Color(0.7f, 0.7f, 0.7f);
            }
            else
            {
                mode = FlybyMenu.FlybyLineMode.Bezier;
                meshStandardColor = new Color(0.425f, 0.8f, 0.6f);
                meshHighlightColor = new Color(0.7f, 0.9f, 0.8f);
            }

            flybyMenu.UpdateLineMode(Index, mode);
            lineRenderer.gameObject.SetActive(!switchToLinear);
            interactableObject.isGrabbable = !switchToLinear;
        }

        public void UpdateBezierControlPolygon(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            lineRenderer.SetPositions(new Vector3[] { p0, p1, p2 });
        }
    }
}
