using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuButtonsManager : MonoBehaviour
{
    public EventSystem eventSystem;

    //------------------//
    // Button Utilities //
    //------------------//
    public void OnMouseOver(GameObject buttonObject)
    {
        Button button = buttonObject.GetComponent<Button>();
        if (button != null)
        {
            button.Select();
        }
    }

    public void OnPlayButtonClicked()
    {
        Debug.Log("Play button clicked.");
        SceneManager.LoadScene(Scenes.mainMenu);
    }
}
