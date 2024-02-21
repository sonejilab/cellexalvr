using CellexalVR.General;
using CellexalVR.Interaction;
using UnityEngine;
namespace CellexalVR.Menu.Buttons
{
    /// <summary>
    /// Abstract general purpose class that represents a tool button on the menu.
    /// Tool buttons have to keep track of the controller model unlike the other buttons.
    /// </summary>
    public abstract class CellexalToolButton : CellexalButton
    {
        protected ControllerModelSwitcher controllerModelSwitcher;
        public Sprite activatedTexture = null;
        public Sprite highlightActivatedTexture = null;
        [HideInInspector]
        public Color meshActivatedColor = Color.blue;
        public bool toolActivated;

        protected abstract ControllerModelSwitcher.Model ControllerModel
        {
            get;
        }

        protected override void Awake()
        {
            base.Awake();
            controllerModelSwitcher = referenceManager.controllerModelSwitcher;
            CellexalEvents.ModelChanged.AddListener(UpdateButton);
            CellexalEvents.GraphsLoaded.AddListener(TurnOn);
            CellexalEvents.ScarfObjectLoaded.AddListener(TurnOn);
            CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
        }

        private void Start()
        {
            SetButtonActivated(false);
        }

        public override void Click()
        {
            bool toolActivated = controllerModelSwitcher.DesiredModel == ControllerModel;
            if (toolActivated)
            {
                controllerModelSwitcher.TurnOffActiveTool(true);
                spriteRenderer.sprite = standardTexture;
                this.toolActivated = false;
            }
            else
            {
                controllerModelSwitcher.DesiredModel = ControllerModel;
                controllerModelSwitcher.ActivateDesiredTool();
                spriteRenderer.sprite = activatedTexture;
                this.toolActivated = true;
                SetHighlighted(false);
            }
        }

        public override void SetHighlighted(bool highlight)
        {
            if (highlight && !toolActivated)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = highlightedTexture;
                }
                else if (meshRenderer != null)
                {
                    meshRenderer.material.color = meshHighlightColor;
                }
            }
            if (!highlight && !toolActivated)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = standardTexture;
                }
                else if (meshRenderer != null)
                {
                    meshRenderer.material.color = meshStandardColor;
                }
            }
            if (highlight && toolActivated)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = highlightActivatedTexture;
                }
                else if (meshRenderer != null)
                {
                    meshRenderer.material.color = meshActivatedColor;
                }
            }
            if (!highlight && toolActivated)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = activatedTexture;
                }
                else if (meshRenderer != null)
                {
                    meshRenderer.material.color = meshActivatedColor;
                }
            }
            if (infoMenu)
            {
                infoMenu.SetActive(highlight);
            }
        }

        public override void SetButtonActivated(bool activate)
        {

            base.SetButtonActivated(activate);

            if (toolActivated && activate)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = activatedTexture;
                }
                else if (meshRenderer != null)
                {
                    meshRenderer.material.color = meshActivatedColor;
                }
            }
        }

        private void UpdateButton()
        {

            if (controllerModelSwitcher.ActualModel != ControllerModel)
            {
                if (buttonActivated)
                {
                    spriteRenderer.sprite = standardTexture;
                }
                if (!buttonActivated)
                {
                    spriteRenderer.sprite = deactivatedTexture;
                }

                toolActivated = false;
            }
        }

        protected void TurnOn()
        {
            SetButtonActivated(true);
        }

        protected void TurnOff()
        {
            SetButtonActivated(false);
        }
    }
}