using UnityEngine;
using UnityEngine.UI;

public class ControlledUI : MonoBehaviour
{
    public Cursor PlayerCursor;

    public Menu[] PlayerMenus; //0 (Unit Actions), 1 (Weapon Choice), 2 (Forecast), 3 (Pause), 4 (Settings)

    public void ActivateMenu(int MenuID)
    {
        PlayerMenus[MenuID].OpenMenu();

        PlayerMenus[MenuID].MenuActive = true;

        for (int i = 0; i < PlayerMenus.Length; i++)
        {
            if (i != MenuID)
            {
                PlayerMenus[i].MenuActive = false;
            }
        }
    }

    public void CloseMenus()
    {
        for (int i = 0; i < PlayerMenus.Length; i++)
        {
            PlayerMenus[i].MenuActive = false;
            PlayerMenus[i].gameObject.SetActive(false);
        }
    }

    public void SetMenuColors(Player Leader) //sets the color of each menu
    {
        Image[] AllMenuImages = GetComponentsInChildren<Image>(true);

        for (int i = 0; i < AllMenuImages.Length; i++)
        {
            if (AllMenuImages[i].tag == "AllyColored")
            {
                AllMenuImages[i].color = Leader.TeamColor;
            }
        }
    }
}
