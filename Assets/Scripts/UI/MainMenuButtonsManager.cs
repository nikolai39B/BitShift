using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;

public class MainMenuButtonsManager : MonoBehaviour
{
    public void Start()
    {
        uiButtons = new List<RectTransform>();
        uiButtons.Add(playButton);
        uiButtons.Add(journalButton);
        uiButtons.Add(recordsButton);
        uiButtons.Add(settingsButton);
    }

    //---------//
    // Buttons //
    //---------//
    public RectTransform playButton;
    public RectTransform journalButton;
    public RectTransform recordsButton;
    public RectTransform settingsButton;

    private List<RectTransform> uiButtons;

    //------------------//
    // Button Utilities //
    //------------------//
    public void OnMouseOver()
    {
        
    }
}
