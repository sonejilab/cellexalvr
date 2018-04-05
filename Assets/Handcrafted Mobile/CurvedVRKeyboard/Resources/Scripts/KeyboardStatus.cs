using System;
using UnityEngine;

namespace CurvedVRKeyboard
{

    [SelectionBase]
    public class KeyboardStatus : KeyboardComponent
    {

        //-----------SET IN UNITY --------------
        [SerializeField]
        public string output;
        [SerializeField]
        public int maxOutputLength;
        [SerializeField]
        public KeyboardOutput keyboardOutput;
        public AutoCompleteList autoCompleteList;

        private ReferenceManager referenceManager;
        private CellManager cellManager;
        private PreviousSearchesList previousSearchesList;

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
        public void HandleClick(KeyboardItem clicked)
        {
            string value = clicked.GetValue();
            // print(value);
            if (value.Equals(QEH) || value.Equals(ABC))
            {                 // special signs pressed
                ChangeSpecialLetters();
            }
            else if (value.Equals(UP) || value.Equals(LOW))
            {         // upper/lower case pressed
                LowerUpperKeys();
            }
            else if (value.Equals(SPACE))
            {
                var type = autoCompleteList.LookUpName(output);
                keyboardOutput.SendToTarget();
                keyboardOutput.Clear();
                autoCompleteList.ClearList();
                //referenceManager.gameManager.InformColorGraphsByGene(output);
                output = "";
            }
            else if (value.Equals(BACK))
            {
                BackspaceKey();
            }
            else
            {        // Normal letter
                TypeKey(value[0]);
            }
        }

        public void SetVars(ReferenceManager referenceManager)
        {
            this.referenceManager = referenceManager;
            previousSearchesList = referenceManager.previousSearchesList;
            cellManager = referenceManager.cellManager;
            autoCompleteList = referenceManager.autoCompleteList;
            keyboardOutput = referenceManager.keyboardOutput;
        }


        /// <summary>
        /// Displays special signs
        /// </summary>
        private void ChangeSpecialLetters()
        {
            KeyLetterEnum ToDisplay = areLettersActive ? KeyLetterEnum.NonLetters : KeyLetterEnum.LowerCase;
            areLettersActive = !areLettersActive;
            isLowercase = true;
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i].SetKeyText(ToDisplay);
            }
        }

        /// <summary>
        /// Changes between lower and upper keys
        /// </summary>
        private void LowerUpperKeys()
        {
            KeyLetterEnum ToDisplay = isLowercase ? KeyLetterEnum.UpperCase : KeyLetterEnum.LowerCase;
            isLowercase = !isLowercase;
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i].SetKeyText(ToDisplay);
            }
        }

        private void BackspaceKey()
        {
            if (output.Length >= 1)
            {
                keyboardOutput.RemoveLetter();
                output = output.Remove(output.Length - 1, 1);
                autoCompleteList.KeyboardOutput = output;
            }
        }

        private void TypeKey(char key)
        {
            if (output.Length < maxOutputLength)
            {
                keyboardOutput.AddLetter(key);
                output = output + key.ToString();
                autoCompleteList.KeyboardOutput = output;
            }

        }

        public void SetKeys(KeyboardItem[] keys)
        {
            this.keys = keys;
        }

        public void setOutput(ref string stringRef)
        {
            output = stringRef;
            keyboardOutput.SetText(stringRef);
        }
    }
}
