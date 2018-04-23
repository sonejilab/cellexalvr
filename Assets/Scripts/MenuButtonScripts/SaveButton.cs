using UnityEngine;
using BayatGames.SaveGameFree.Examples;

/// <summary>
/// Represents the button that saves the current scene.
/// </summary>
public class SaveButton : CellexalButton
{

    public SaveScene saveScene;
    public Sprite gray;
    public Sprite original;
    private float elapsedTime;
    private float time = 1.0f;
    private bool changeSprite;

    // Use this for initialization
    protected override string Description
    {
        get { return "Save Session"; }
    }

    protected override void Update()
    {
        base.Update();
        if (changeSprite)
        {
            if (elapsedTime < time)
            {
                elapsedTime += Time.deltaTime;
            }
            else
            {
                standardTexture = original;
                changeSprite = false;
            }
        }
    }

    // Update is called once per frame
    protected override void Click()
    {
        Debug.Log("Do Save");
        saveScene.Save();
        elapsedTime = 0.0f;
        standardTexture = gray;
        changeSprite = true;
    }
    /*
	void ChangeSprite() 
	{
		spriteRenderer.sprite = gray;
		float elapsedTime = 0.0f;
		if (elapsedTime > time) 
		{
			spriteRenderer.sprite = original;
		} else {
			elapsedTime += Time.deltaTime;
		}
	}*/

}
