
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CurvedVRKeyboard {

    /// <summary>
    /// Creates Keyboard, calculates all necessary positions and rotations
    /// </summary>
    [System.Serializable]
    [ExecuteInEditMode]
    public class KeyboardCreator : KeyboardComponent {

        //-----------SET IN UNITY --------------
        [SerializeField]
        private float curvature;
        [SerializeField]
        private Transform raycastingSource;
        [SerializeField]
        private string clickHandle;
        [SerializeField]
        private Material keyNormalMaterial;
        [SerializeField]
        private Material keySelectedMaterial;
        [SerializeField]
        private Material keyPressedMaterial;
        [SerializeField]
        private Sprite spaceSprite;
        [SerializeField]
        private KeyboardItem space;
        [SerializeField]
        private float referencedPixels = 1f;

        //-------private Calculations---------
        private readonly float defaultSpacingColumns = 56.3f;
        private readonly float defaultSpacingRows = 1.0f;
        private readonly float defaultRotation = 90f;
        public float centerPointDistance = -1f;
        private KeyboardItem[] keys;
        private int row;

        //--------------others----------------
        private ErrorReporter errorReporter;
        private const string MESH_NAME_SEARCHED = "Quad";
        public bool wasStaticOnStart;
        private const int spaceKeyNumber = 28;
        private const float radius = 3;

        //--------------borders of sprite  -----
        private float leftBorder;
        private float rightBorder;
        private float topBorder;
        private float bottomBorder;



        public void Awake() {
            InitKeys();
            ChangeMaterialOnKeys();
            if (!Application.isPlaying) {
                ManageKeys();
            }
            wasStaticOnStart = gameObject.isStatic;
            SetComponents();

        }

        public void ManageKeys() {
            checkErrors();

            if (!errorReporter.IsErrorPresent()) {
                if (centerPointDistance == -1f) {
                    CurvatureToDistance();
                }
                FillAndPlaceKeys();
            }
        }

        public void InitKeys() {
            if (keys == null || KeyboardItem.forceInit) {
                List<KeyboardItem> allKeys = new List<KeyboardItem>(GetComponentsInChildren<KeyboardItem>());
                for (int i = 0; i < allKeys.Count; i++) {
                    allKeys[i].Position = i;
                    allKeys[i].Init();
                }
                space = allKeys[spaceKeyNumber];
                keys = allKeys.ToArray();
            }
            space.ManipulateSpace(this, SpaceSprite);
        }

        /// <summary>
        /// Sets values for other necessary components
        /// </summary>
        private void SetComponents() {
            KeyboardRaycaster rayCaster = GetComponent<KeyboardRaycaster>();
            rayCaster.SetRaycastingTransform(RaycastingSource);
            rayCaster.SetClickButton(ClickHandle);
            rayCaster.SetTarget(gameObject);
            KeyboardStatus status = GetComponent<KeyboardStatus>();
            status.SetKeys(keys);
        }

        /// <summary>
        /// Fills key with text and calculates position 
        /// </summary>
        private void FillAndPlaceKeys() {
            foreach (KeyboardItem key in keys) {
                key.SetKeyText(KeyboardItem.KeyLetterEnum.LowerCase);
                PositionSingleLetter(key);
            }
        }

        /// <summary>
        /// Calculates whole transformation for single key
        /// Whole idea is to create circle and place keys on it
        /// if keys are in straight line, it means that deegre 
        /// between step is really low. 
        /// </summary>
        /// <param name="iteration">index of key to be placed</param>
        /// <param name="keyTransform">key transformation</param>
        private void PositionSingleLetter(KeyboardItem key)
        {
            int iteration = key.Position;
            Transform keyTransform = key.transform;
            // Check row and how many keys were placed
            float keysPlaced = CalculateKeyOffsetAndRow(iteration);
            float degree = CalculateRotation(lettersInRowsCount[row] - 1, iteration - keysPlaced);
            //caluclate position on cylinder with circle equation formula
            //http://www.mathopenref.com/coordparamcircle.html
            key.transform.localPosition = CalculatePositionOnCylinder(degree);
            //rotate keys by their placement angle
            key.transform.localEulerAngles = new Vector3(0, -degree * Mathf.Rad2Deg - 90f, 0);
            // keys are moved from center couse of increasing circle radius,
            // so position must be restored to radius
            key.transform.localPosition = RestorePosition(key);
        }

        public float CalculateRotation(float rowSize, float offset)
        {
            // Calculate degree of single key on cricle
            return Mathf.Deg2Rad * (defaultRotation + rowSize
                * SpacingBetweenKeys / 2 - offset
                * SpacingBetweenKeys);
        }

        private float CalculateKeyOffsetAndRow ( int iteration ) {
            float keysPlaced = 0;
            row = 0;
            int iterationCounter = 0;
            for(int rowChecked = 0;rowChecked <= 2;rowChecked++) {
                iterationCounter += lettersInRowsCount[rowChecked];
                if(iteration >= iterationCounter) {
                    keysPlaced += lettersInRowsCount[rowChecked];
                    row++;
                }
            }
            //last row with space requires special calculations
            if(iteration >= iterationCounter) {
                const float offsetBetweenSpecialKeys = 1.5f;
                keysPlaced -= ( iteration - iterationCounter ) * offsetBetweenSpecialKeys;
            }
            return keysPlaced;
        }

        public Vector3 CalculatePositionOnCylinder(float degree)
        {
            return new Vector3(
                Mathf.Cos(degree) * centerPointDistance,
                -row * RowSpacing,
                Mathf.Sin(degree) * centerPointDistance);
        }

        private Vector3 RestorePosition(KeyboardItem key)
        {
            return new Vector3(
                key.transform.localPosition.x,
                key.transform.localPosition.y,
                key.transform.localPosition.z - centerPointDistance + radius);
        }

        /// <summary>
        /// tan (x * 1,57) - tan is in range of <0,3.14>. With
        /// this approach we can scale it to range <0(0),1(close to infinity)>.
        /// Why + radious = 3?? because virtual radius of our circle is 3 
        /// google (tan(x*1.57) + 3) for visualization
        /// Higher values make center position further from keys (straight line)
        /// </summary>
        private void CurvatureToDistance () {
            centerPointDistance = Mathf.Tan((curvature) * 1.57f) + radius;
        }

        /// <summary>
        /// Changes materials for all keys
        /// </summary>
        public void ChangeMaterialOnKeys () {
            foreach(KeyboardItem key in keys) {
                key.SetMaterials(KeyNormalMaterial, KeySelectedMaterial, KeyPressedMaterial);
            }
        }

        public void checkErrors () {
            errorReporter = ErrorReporter.Instance;
            errorReporter.Reset();
            if(keys.Length != KEY_NUMBER) {//is there correct number of keys
                errorReporter.SetMessage("Cannot procced. Number of keys is incorrect. Revert your changes to prefab", ErrorReporter.Status.Error);
                return;
            }
            if(space == null) { // project improted over older package crashes without this 
                space = keys[spaceKeyNumber];
            }
            if(space.GetMeshName().Equals(MESH_NAME_SEARCHED)) {//are keys positioned corectly
                errorReporter.SetMessage("Cannot  procced. Space key data is incorrect. Revert your changes to prefab or place keys in correct sequence", ErrorReporter.Status.Error);
                return;
            }
            if(!gameObject.GetComponent<KeyboardStatus>().isReflectionPossible) {
                errorReporter.SetMessage("GameObject Output is not set, or there is no script with \"text\" property on current gameobject", ErrorReporter.Status.Warning);
                return;
            }
            if(wasStaticOnStart && Application.isPlaying) {//is playing and was static when play mode started
                errorReporter.SetMessage("If editng during gameplay is necessary, quit gameplay and remove static flag from keyboard and its children."
                    + " Reamember to set keyboard to static when building", ErrorReporter.Status.Info);
                return;
            }
            CheckKeyArrays();
        }

        /// <summary>
        /// When spacebar material is set it is created as a new material so the reference 
        /// to buttons' material is lost and changing them do not affect spacebar. 
        /// User has to manualy reload material if he changed them in editor
        /// </summary>
        public void ReloadSpaceMaterials () {
            space.SetMaterials(KeyNormalMaterial, KeySelectedMaterial, KeyPressedMaterial);
            space.ManipulateSpace(this, SpaceSprite);
        }


        //---------------PROPERTIES----------------
        public float Curvature {
            get {
                return 1f - curvature;
            }
            set {
                if(curvature != ( 1f - value )) {// Value changed
                    curvature = 1f - value;
                    CurvatureToDistance();
                    ManageKeys();
                    space.ManipulateSpace(this, spaceSprite);
                } 
            }
        }

        public float SpacingBetweenKeys {
            get {
                return defaultSpacingColumns / centerPointDistance;
            }
        }

        public float RowSpacing {
            get {
                return defaultSpacingRows ;
            }
        }


        public Material KeyNormalMaterial {
            get {
                return keyNormalMaterial;
            }
            set {
                if(KeyNormalMaterial != value) {
                    keyNormalMaterial = value;
                    foreach(KeyboardItem key in keys) {
                        key.SetMaterial(KeyboardItem.KeyMaterialEnum.Normal, keyNormalMaterial);
                    }
                }
            }
        }

        public Material KeySelectedMaterial {
            get {
                return keySelectedMaterial;
            }
            set {
                if(keySelectedMaterial != value) {
                    keySelectedMaterial = value;
                    foreach(KeyboardItem key in keys) {
                        key.SetMaterial(KeyboardItem.KeyMaterialEnum.Selected, keySelectedMaterial);
                    }
                }

            }
        }

        public Material KeyPressedMaterial {
            get {
                return keyPressedMaterial;
            }
            set {
                if(KeyPressedMaterial != value) {
                    keyPressedMaterial = value;
                    foreach(KeyboardItem key in keys) {
                        key.SetMaterial(KeyboardItem.KeyMaterialEnum.Pressed, keyPressedMaterial);
                    }
                }
            }
        }

        public Sprite SpaceSprite {
            get {
                return spaceSprite;
            }
            set {
                //if there was a sprite and now it changed to null
                if(spaceSprite != value && value == null) {
                    spaceSprite = value;
                    space.ManipulateSpace(this, SpaceSprite);
                    space.SetMaterials(KeyNormalMaterial, KeySelectedMaterial, KeyPressedMaterial);
                }
                //if value has changed and it's not null 
                else if(value != null) {
                    if(SpaceSprite != value || AreBordersChanged(value)) {//if new or borders changed
                        spaceSprite = value;
                        ChangeBorders(SpaceSprite.border);
                        space.ManipulateSpace(this, SpaceSprite);
                        space.SetMaterials(KeyNormalMaterial, KeySelectedMaterial, KeyPressedMaterial);
                    }
                }
            }
        }


        public Transform RaycastingSource {
            get {
                return raycastingSource;
            }
            set {
                if(raycastingSource != value) {
                    InitKeys();
                    raycastingSource = value;
                    KeyboardRaycaster rayCaster = GetComponent<KeyboardRaycaster>();
                    rayCaster.SetRaycastingTransform(RaycastingSource);
                }
            }
        }

        public string ClickHandle {
            get {
                return clickHandle;
            }
            set {
                clickHandle = value;
                KeyboardRaycaster rayCaster = GetComponent<KeyboardRaycaster>();
                rayCaster.SetClickButton(clickHandle);
            }
        }

        public float ReferencedPixels {
            get {
                return referencedPixels;
            }
            set {
                if(ReferencedPixels != value) {
                    referencedPixels = value <= 0.01f ? 0.01f : value;
                    space.ManipulateSpace(this, SpaceSprite);
                }
            }
        }
        /// <summary>
        ///  Borders setup changes cannot be automatically detected so we have to do this manually
        /// </summary>
        /// <param name="newBorder"></param>
        private bool AreBordersChanged (Sprite newSprite) {
            Vector4 newBorder = newSprite.border;
            if(leftBorder != newBorder.x || bottomBorder != newBorder.y || rightBorder != newBorder.z || topBorder != newBorder.w) {
                ChangeBorders(newBorder);
                return true;
            }
            return false;
        }

        private void ChangeBorders ( Vector4 newBorder ) {
            leftBorder = newBorder.x;
            bottomBorder = newBorder.y;
            rightBorder = newBorder.z;
            topBorder = newBorder.w;
        }
    }
}



