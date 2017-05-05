using System;
using UnityEngine;
public class Cell
	{
		private string label;

	public Cell (string label) {
		this.label = label;
	}

	/*public void setLabel(string label)
	{
		this.label = label;
	}*/

    public string Label
    {
        get
        {
            return this.label;
        }
        set
        {
            this.label = value;
        }
    }
}
