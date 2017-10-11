
public class ArcsTabButton : TabButton
{
    [UnityEngine.HideInInspector]
    public NetworkHandler Handler;

    public override void SetHighlighted(bool highlight)
    {
        base.SetHighlighted(highlight);
        if (highlight)
        {
            Handler.Highlight();
        }
        else
        {
            Handler.Unhighlight();
        }
    }
}

