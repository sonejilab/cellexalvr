using UnityEngine;
/// <summary>
/// Class to represent a simple calculator.
/// </summary>
public class Calculator : MonoBehaviour
{
    public TextMesh outputText;

    private bool clearDisplayBeforeNextDigit;
    private bool addDecimal = false;
    private int decimalDivider = 10;
    // What number is displayed on the display if result is not displayed.
    private float tempDisplayed = 0f;
    private float result = 0f;
    private float memory = 0f;

    private Operator nextOp;
    public enum Operator { NONE, ADD, SUB, MUL, DIV, EQ }

    private void Start()
    {
        //outputText.text = "0";
    }

    /// <summary>
    /// Adds a digit to the display.
    /// </summary>
    /// <param name="digit">The digit to add</param>
    public void AddDigitToDisplay(int digit)
    {
        if (clearDisplayBeforeNextDigit)
        {
            outputText.text = "";
            clearDisplayBeforeNextDigit = false;
        }

        if (addDecimal)
        {
            tempDisplayed += ((float)digit / decimalDivider);
            decimalDivider *= 10;
        }
        else
        {
            tempDisplayed = tempDisplayed * 10 + digit;
        }
        outputText.text = outputText.text + digit;
    }

    /// <summary>
    /// Called when the decimal button is pressed.
    /// </summary>
    public void Decimal()
    {
        if (!addDecimal)
        {
            addDecimal = true;
            decimalDivider = 10;
            outputText.text += ".";
        }
    }

    /// <summary>
    /// Clears the display and current result.
    /// </summary>
    public void Clear()
    {
        nextOp = Operator.NONE;
        result = 0f;
        tempDisplayed = 0f;
        outputText.text = "";
        addDecimal = false;
        decimalDivider = 10;
    }

    /// <summary>
    /// Checks if there is no result yet. Sets the result to what is on the display.
    /// </summary>
    private void CheckEmptyResult()
    {
        if (result == 0f)
        {
            result = tempDisplayed;
            tempDisplayed = 0f;
        }
    }

    /// <summary>
    /// Executes a queued operation.
    /// </summary>
    private void DoQueuedOp()
    {

        switch (nextOp)
        {
            case Operator.NONE:
                return;
            case Operator.ADD:
                result += tempDisplayed;
                break;
            case Operator.SUB:
                result -= tempDisplayed;
                break;
            case Operator.MUL:
                result *= tempDisplayed;
                break;
            case Operator.DIV:
                result /= tempDisplayed;
                break;
        }
        nextOp = Operator.NONE;
        outputText.text = result.ToString();
    }

    /// <summary>
    /// Clears the display.
    /// </summary>
    private void ClearTempDisplay()
    {
        tempDisplayed = 0f;
        clearDisplayBeforeNextDigit = true;
        decimalDivider = 10;
        addDecimal = false;
    }

    /// <summary>
    /// Called when the addition button is pressed.
    /// </summary>
    public void DoAdd()
    {
        DoQueuedOp();
        nextOp = Operator.ADD;
        CheckEmptyResult();
        ClearTempDisplay();
    }

    /// <summary>
    /// Called when the subtraction button is pressed.
    /// </summary>
    public void DoSub()
    {
        DoQueuedOp();
        nextOp = Operator.SUB;
        CheckEmptyResult();
        ClearTempDisplay();
    }

    /// <summary>
    /// Called when the multiplication button is pressed.
    /// </summary>
    public void DoMul()
    {
        DoQueuedOp();
        nextOp = Operator.MUL;
        CheckEmptyResult();
        ClearTempDisplay();
    }

    /// <summary>
    /// Called when the division button is pressed.
    /// </summary>
    public void DoDiv()
    {
        DoQueuedOp();
        nextOp = Operator.DIV;
        CheckEmptyResult();
        ClearTempDisplay();
    }

    /// <summary>
    /// Called when the M+ button is pressed. Adds the number on the display to the value in the memory.
    /// </summary>
    public void DoMemAdd()
    {
        memory += tempDisplayed;
        outputText.text = memory.ToString();
        clearDisplayBeforeNextDigit = true;
    }

    /// <summary>
    /// Called when the M- button is pressed. Subtracts the number on the display to the value in the memory.
    /// </summary>
    public void DoMemSub()
    {
        memory -= tempDisplayed;
        outputText.text = memory.ToString();
        clearDisplayBeforeNextDigit = true;
    }

    /// <summary>
    /// Called when the MC button is pressed. Clears the number stored int he memory.
    /// </summary>
    public void DoMemClear()
    {
        memory = 0f;
    }

    /// <summary>
    /// Called when the equals button is pressed.
    /// </summary>
    public void DoEq()
    {
        DoQueuedOp();
    }
}

