using CellexalVR.Menu.Buttons.Facs;
using UnityEngine;
namespace CellexalVR.Menu.SubMenus
{
    /// <summary>
    /// Represents the sub menu that pops up when the <see cref="ColorByIndexButton"/> is pressed.
    /// </summary>
    public class ColorByIndexMenu : MenuWithTabs
    {
        protected Color[] colors = new Color[0];
        protected Color[] Colors
        {
            get
            {
                return colors;
            }
        }

        /// <summary>
        /// Create the buttons that represent the different indices.
        /// </summary>
        /// <param name="names">The name if the facs measurements.</param>
        public override void CreateButtons(string[] names)
        {
            base.CreateButtons(names);

            for (int i = 0; i < cellexalButtons.Count; ++i)
            {
                var b = cellexalButtons[i].GetComponent<ColorByIndexButton>();
                b.referenceManager = referenceManager;
                b.SetIndex(names[i]);
                b.gameObject.name = names[i];
                b.parentMenu = this;
            }
        }

    }
}