using System.IO;
using UnityEngine;

/// <summary>
/// Represents the menu that controls the flashing genes.
/// </summary>
public class FlashGenesMenu : MenuWithTabs
{
    public TextMesh loadingText;

    // used for overriding some values
    public FlashGenesMenu()
    {
        tabButtonPos = new Vector3(-3.168f, 5.44f, 6.46f);
        tabButtonPosOriginal = tabButtonPos;
        tabButtonPosInc = new Vector3(1.7f, 0, 0);
    }

    protected override void Start()
    {
        base.Start();
        CellexalEvents.GraphsUnloaded.AddListener(OnGraphsUnloaded);
        CellexalEvents.FlashGenesFileStartedLoading.AddListener(ShowLoadingText);
        CellexalEvents.FlashGenesFileFinishedLoading.AddListener(HideLoadingText);
    }

    /// <summary>
    /// Creates one tab for each .fgv file in the data folder.
    /// </summary>
    /// <param name="dataFolderPath"> The path to the folder where the .fgv files are. </param>
    public void CreateTabs(string dataFolderPath)
    {
        string[] files = Directory.GetFiles(dataFolderPath, "*.fgv");
        foreach (string file in files)
        {
            FlashGenesTab newTab = (FlashGenesTab)AddTab(tabPrefab);
            newTab.GeneFilePath = file;

            // Read the categories from the file.
            FileStream fileStream = new FileStream(file, FileMode.Open);
            StreamReader streamReader = new StreamReader(fileStream);
            string[] categories = streamReader.ReadLine().Split(',');
            for (int i = 0; i < categories.Length; ++i)
            {
                categories[i] = categories[i].Trim();
            }
            streamReader.Close();
            fileStream.Close();
            newTab.CreateCategoryButtons(categories);
        }
        TurnOffAllTabs();
        /*if (tabs.Count > 0)
        {
            tabs[tabs.Count - 1].SetTabActive(true);
        }*/
    }

    private void OnGraphsUnloaded()
    {
        DestroyTabs();
    }

    private void ShowLoadingText()
    {
        loadingText.gameObject.SetActive(true);
    }

    private void HideLoadingText()
    {
        loadingText.gameObject.SetActive(false);
    }

}
