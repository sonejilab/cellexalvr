using System.Linq;


namespace CellexalVR.Interaction
{
    public class NumericalKeyboardHandler : KeyboardHandler
    {
        public override string[][] Layouts { get; protected set; } = {
            new string[] { "7", "8", "9",
                           "4", "5", "6",
                           "1", "2", "3", "Back",
                           "0", ".", "%", "Clear",
                           "Enter" }
        };

        /// <summary>
        /// Validates the currently written text.
        /// </summary>
        public void ValidateInput(string text)
        {
            // text must only contain one dot
            int indexOfSecondDot = IndexOfSecond(text, '.');
            if (indexOfSecondDot != -1)
            {
                text = text.Substring(0, indexOfSecondDot);
            }

            // nothing must be typed after a percent sign
            int indexOfPercent = text.IndexOf('%');
            if (indexOfPercent != -1)
            {
                text = text.Substring(0, indexOfPercent + 1);
            }
            SetAllOutputs(text);
        }

        private int IndexOfSecond(string s, char c)
        {
            bool firstFound = false;
            for (int i = 0; i < s.Length; ++i)
            {
                if (s[i] == c)
                {
                    if (!firstFound)
                    {
                        firstFound = true;
                    }
                    else
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
    }
}