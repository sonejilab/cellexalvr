using UnityEngine;


namespace CurvedVRKeyboard {

    [SelectionBase]
    public class KeyboardStatus: KeyboardComponent {

        //-----------SET IN UNITY --------------
        [SerializeField]
        public string  output;
        [SerializeField]
        public int maxOutputLength;
        [SerializeField]
        public GameObject targetGameObject;

        private GraphManager graphManager;


        //----CurrentKeysStatus----
        [SerializeField]
        public Component typeHolder;
        [SerializeField]
        public bool isReflectionPossible;
        private KeyboardItem[] keys;
        private bool areLettersActive = true;
        private bool isLowercase = true;
        private const char BLANKSPACE = ' ';
        private const string TEXT = "text";
        private Component textComponent;


        /// <summary>
        /// Handles click on keyboarditem
        /// </summary>
        /// <param name="clicked">keyboard item clicked</param>
        public void HandleClick ( KeyboardItem clicked ) {
            string value = clicked.GetValue();
            if(value.Equals(QEH) || value.Equals(ABC)) { // special signs pressed
                ChangeSpecialLetters();
            } else if(value.Equals(UP) || value.Equals(LOW)) { // upper/lower case pressed
                LowerUpperKeys();
            } else if(value.Equals(SPACE)) {
                graphManager.colorAllGraphsByGene(output);
                output = "";
                textComponent = targetGameObject.GetComponent(typeHolder.GetType());
                textComponent.GetType().GetProperty(TEXT).SetValue(textComponent, output, null);
            } else if(value.Equals(BACK)) {
                BackspaceKey();
            } else {// Normal letter
                TypeKey(value[0]);
            }
        }

        public void setGraphManager(GraphManager manager)
        {
            graphManager = manager;
        }

        /// <summary>
        /// Displays special signs
        /// </summary>
        private void ChangeSpecialLetters () {
            KeyLetterEnum ToDisplay = areLettersActive ? KeyLetterEnum.NonLetters : KeyLetterEnum.LowerCase;
            areLettersActive =!areLettersActive;
            isLowercase = true;
            for(int i = 0;i < keys.Length;i++) {
                keys[i].SetKeyText(ToDisplay);
            }
        }

        /// <summary>
        /// Changes between lower and upper keys
        /// </summary>
        private void LowerUpperKeys () {
            KeyLetterEnum ToDisplay = isLowercase ? KeyLetterEnum.UpperCase : KeyLetterEnum.LowerCase;
            isLowercase = !isLowercase;
            for(int i = 0;i < keys.Length;i++) {
                keys[i].SetKeyText(ToDisplay);
            }
        }

        private void BackspaceKey () {
            if(output.Length >= 1) {
                textComponent = targetGameObject.GetComponent(typeHolder.GetType());
                textComponent.GetType().GetProperty(TEXT).SetValue(textComponent, output.Remove(output.Length - 1, 1), null);
                output = output.Remove(output.Length - 1, 1);
            }
        }

        private void TypeKey ( char key ) {
            if(output.Length < maxOutputLength) {
                textComponent = targetGameObject.GetComponent(typeHolder.GetType());
                textComponent.GetType().GetProperty(TEXT).SetValue(textComponent, output + key.ToString(),null);
                output = output + key.ToString();
            }
                
        }

        public void SetKeys ( KeyboardItem[] keys ) {
            this.keys = keys;
        }

        public void setOutput (ref string stringRef) {
            output = stringRef;
        }
    }
}
