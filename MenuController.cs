using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public int SelectedButton;

    public RectTransform MenuCursor;

    public bool MainCursorActive; //the cursor deactivates if another menu is open

    // Start is called before the first frame update
    void Start()
    {
        MainCursorActive = true;
        MoveMenuCursor(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (MainCursorActive)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) && SelectedButton > 0)
            {
                MoveMenuCursor(SelectedButton - 1);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) && SelectedButton < 3)
            {
                MoveMenuCursor(SelectedButton + 1);
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                MenuCursorSelect();
            }
        }
    }

    void MoveMenuCursor(int NewLocation)
    {
        SelectedButton = NewLocation;

        MenuCursor.anchoredPosition = new Vector3(-50f, -200f + (-300f * NewLocation), 0f);
    }

    void MenuCursorSelect()
    {
        switch (SelectedButton)
        {
            case 0: //Create Room... spawn for now
                {
                    OpenRoom();
                    break;
                }

            case 1: //Join Room
                {
                    OpenRoom();
                    break;
                }

            case 2: //My Units menu
                {
                    SceneManager.LoadSceneAsync(1);
                    break;
                }

            case 3: //Settings menu
                {
                    SceneManager.LoadSceneAsync(1);
                    break;
                }
        }
    }

    void OpenRoom()
    {
        MainCursorActive = false;
    }
}
