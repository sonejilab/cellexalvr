using UnityEngine;
using UnityEngine.UI;

namespace CurvedVRKeyboard {
    public class KeyboardItem : KeyboardComponent {
        private Text letter;

        public static bool forceInit = true;
        private int position;

        //------Click-------
        private bool clicked = false;
        private float clickHoldTimer = 0f;
        private float clickHoldTimeLimit = 0.15f;

        //-----Materials-----
        [SerializeField, HideInInspector]
        private Material keyNormalMaterial;
        [SerializeField, HideInInspector]
        private Material keySelectedMaterial;
        [SerializeField, HideInInspector]
        private Material keyPressedMaterial;

        //--Mesh&Renderers---
        [SerializeField, HideInInspector]
        private Sprite spaceSprite;

        private SpaceMeshCreator meshCreator;
        private Renderer quadFront;
        private const string QUAD_FRONT = "Front";
        private const string MAIN_TEXURE_NAME_IN_SHADER = "_MainTex";



        public enum KeyMaterialEnum {
            Normal, Selected, Pressed
        }

        public void Awake() {
            Init();
        }

        public void Init() {
            if(letter == null || quadFront == null) {  // Check if initialized
                letter = gameObject.GetComponentInChildren<Text>();
                quadFront = transform.Find(QUAD_FRONT).GetComponent<Renderer>();
            }
        }

        /// <summary>
        /// Handle for hover function
        /// </summary>
        public void Hovering() {
            if(!clicked) {// Is not already being clicked?
                ChangeDisplayedMaterial(keySelectedMaterial);
            }
            else {
                HoldClick();
            }
        }

        /// <summary>
        /// handle for click begining
        /// </summary>
        public void Click() {
            clicked = true;
            ChangeDisplayedMaterial(keyPressedMaterial);
        }

        /// <summary>
        /// handle after click was started
        /// </summary>
        private void HoldClick() {
            ChangeDisplayedMaterial(keyPressedMaterial);
            clickHoldTimer += Time.deltaTime;
            if(clickHoldTimer >= clickHoldTimeLimit) {// Check if time of click is over
                clicked = false;
                clickHoldTimer = 0f;
            }
        }

        /// <summary>
        /// Handle for hover over
        /// </summary>
        public void StopHovering() {
            ChangeDisplayedMaterial(keyNormalMaterial);
        }

        /// <summary>
        /// Get value of key text
        /// </summary>
        /// <returns>value of key</returns>
        public string GetValue() {
            return letter.text;
        }

        /// <summary>
        /// Changes value of key text
        /// </summary>
        public void SetKeyText(KeyLetterEnum letterType) {
            string value = "";
            switch(letterType) {
                case KeyLetterEnum.LowerCase:
                    value = allLettersLowercase[Position];
                    break;
                case KeyLetterEnum.UpperCase:
                    value = allLettersUppercase[Position];
                    break;
                case KeyLetterEnum.NonLetters:
                    value = allSpecials[Position];
                    break;
            }
            if(!letter.text.Equals(value)) {
                letter.text = value;
            }
        }

        /// <summary>
        /// Changes material on key
        /// </summary>
        /// <param name="material">material to be displayed</param>
        private void ChangeDisplayedMaterial(Material material) {
            quadFront.sharedMaterial = material;
        }

        /// <summary>
        /// Changes materials on all keys
        /// </summary>
        /// <param name="keyNormalMaterial"></param>
        /// <param name="keySelectedMaterial"></param>
        /// <param name="keyPressedMaterial"></param>
        public void SetMaterials(Material keyNormalMaterial, Material keySelectedMaterial, Material keyPressedMaterial) {
            this.keyNormalMaterial = keyNormalMaterial;
            this.keySelectedMaterial = keySelectedMaterial;
            this.keyPressedMaterial = keyPressedMaterial;

            if(Position == POSITION_SPACE) {
                SetMaterial(KeyMaterialEnum.Normal, keyNormalMaterial);
            }

            if(IfSpaceWithSprite()) {
                AddSpriteToMaterial(spaceSprite);
            }
        }

        private bool IfSpaceWithSprite() {
            return spaceSprite != null;
        }

        /// <summary>
        /// Changes material for state
        /// </summary>
        /// <param name="materialEnum">state of which material will be changed</param>
        /// <param name="newMaterial">new material</param>
        public void SetMaterial(KeyMaterialEnum materialEnum, Material newMaterial) {
            Init();
            switch(materialEnum) {
                case KeyMaterialEnum.Normal:
                    keyNormalMaterial = IfSpaceWithSprite() ?
                        ChangeMaterialTexture(spaceSprite, newMaterial) : newMaterial;
                    quadFront.sharedMaterial = keyNormalMaterial;
                    break;
                case KeyMaterialEnum.Selected:
                    keySelectedMaterial = IfSpaceWithSprite() ?
                        ChangeMaterialTexture(spaceSprite, newMaterial) : newMaterial;

                    break;
                case KeyMaterialEnum.Pressed:
                    keyPressedMaterial = IfSpaceWithSprite() ?
                        ChangeMaterialTexture(spaceSprite, newMaterial) : newMaterial;
                    break;
            }
        }

        /// <summary>
        /// Changes 'space' bar mesh
        /// </summary>
        /// <param name="creator"></param>
        public void ManipulateSpace(KeyboardCreator creator, Sprite spaceSprite) {
            this.spaceSprite = spaceSprite;
            if(meshCreator == null)
                meshCreator = new SpaceMeshCreator(creator);
            meshCreator.Recalculate9Slice(spaceSprite,creator.ReferencedPixels);
            
            if (!creator.wasStaticOnStart)
            {
                Init();
                meshCreator.BuildFace(quadFront, true);
            }
        }

        private void AddSpriteToMaterial(Sprite spaceSprite) {
            keyNormalMaterial = ChangeMaterialTexture(spaceSprite, keyNormalMaterial);
            keySelectedMaterial = ChangeMaterialTexture(spaceSprite, keySelectedMaterial);
            keyPressedMaterial = ChangeMaterialTexture(spaceSprite, keyPressedMaterial);

            SetMaterial(KeyMaterialEnum.Normal, keyNormalMaterial);
            SetMaterial(KeyMaterialEnum.Selected, keySelectedMaterial);
            SetMaterial(KeyMaterialEnum.Pressed, keyPressedMaterial);
        }

        private Material ChangeMaterialTexture(Sprite spaceTexture, Material materialToChange) {
            if(materialToChange == null) {
                return materialToChange;
            }

            materialToChange = new Material(materialToChange);
            materialToChange.SetTexture(MAIN_TEXURE_NAME_IN_SHADER, spaceTexture.texture);

            return materialToChange;
        }

        public string GetMeshName() {
            if(quadFront == null) {
                Init();
            }

            return quadFront.GetComponent<MeshFilter>().sharedMesh.name;
        }

        public int Position {
            get {
                return position;
            }

            set {
                position = value;
            }
        }

    }
}


