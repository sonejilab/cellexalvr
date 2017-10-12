using System.IO;
using UnityEngine;

/// <summary>
/// This class represents the menu that controls the flashing genes.
/// </summary>
public class FlashGenesMenu : MenuWithTabs
{
    public FlashGenesTab tabPrefab;
    public TextMesh loadingText;

    public FlashGenesMenu()
    {
        tabButtonPos = new Vector3(-3.168f, 5.44f, 6.46f);
        tabButtonPosInc = new Vector3(1.7f, 0, 0);
    }

    protected override void Start()
    {
        base.Start();
        ButtonEvents.FlashGenesFileStartedLoading.AddListener(ShowLoadingText);
        ButtonEvents.FlashGenesFileFinishedLoading.AddListener(HideLoadingText);
    }

    public void CreateTabs(string dataFolderPath)
    {
        string[] files = Directory.GetFiles(dataFolderPath, "*fgv");
        foreach (string file in files)
        {
            FlashGenesTab newTab = AddTab(tabPrefab);
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
        if (tabs.Count > 0)
        {
            tabs[tabs.Count - 1].SetTabActive(true);
        }
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
