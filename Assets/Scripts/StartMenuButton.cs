
public class StartMenuButton : CellexalButton
{
    public SceneLoader sceneLoader;

    protected override string Description
    {
        get
        {
            return "Back to Start Menu";
        }
    }



    protected override void Click()
    {
        sceneLoader.LoadScene("SceneLoaderTest");
    }
}
/*
void Start()
{
    device = SteamVR_Controller.Input((int)rightController.index);
    spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
}
void OnTriggerEnter(Collider other)
{
    if (other.gameObject.tag == "Controller")
    {
        descriptionText.text = "Back to Start Menu";
        controllerInside = true;
    }
}

void OnTriggerExit(Collider other)
{
    if (other.gameObject.tag == "Controller")
    {
        descriptionText.text = "";
        controllerInside = false;
    }
}*/


