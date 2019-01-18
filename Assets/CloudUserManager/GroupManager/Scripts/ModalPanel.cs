using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

public class ModalPanel : MonoBehaviour {

    public Text messageText;
    public Button abortButton;
    public Button yesButton;
    public Button noButton;
    public GameObject modalPanelObject;

    private static ModalPanel modalPanel;

    public static ModalPanel Instance()
    {
        if (!modalPanel)
        {
            modalPanel = FindObjectOfType(typeof(ModalPanel)) as ModalPanel;
            if (!modalPanel)
                Debug.LogError("There needs to be one active ModalPanel script on a GameObject in your scene.");
        }

        return modalPanel;
    }

    public void ShowProgress(string message = null, UnityAction abortEvent = null)
    {
        modalPanelObject.SetActive(true);

        if (message != null)
        {
            messageText.text = message;
        }

        ShowButton(abortButton, "Cancel", abortEvent);
        HideButton(yesButton);
        HideButton(noButton);
    }

    public void ShowMessage(string message, UnityAction closeEvent = null)
    {
        modalPanelObject.SetActive(true);

        messageText.text = message;

        ShowButton(abortButton, "Close", closeEvent);
        HideButton(yesButton);
        HideButton(noButton);
    }

    public void ShowYesNoDialog(string message, UnityAction yesEvent = null, UnityAction noEvent = null)
    {
        modalPanelObject.SetActive(true);

        messageText.text = message;
        
        ShowButton(yesButton, "Yes", yesEvent);
        ShowButton(noButton, "No", noEvent);
        HideButton(abortButton);
    }

    private void ShowButton(Button btn, string label, UnityAction action)
    {
        btn.gameObject.SetActive(true);

        btn.GetComponentInChildren<Text>().text = label;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(Hide);
        if (action != null)
            btn.onClick.AddListener(action);        
    }

    private void HideButton(Button btn)
    {
        btn.onClick.RemoveAllListeners();
        btn.gameObject.SetActive(false);
    }

    public void Hide()
    {
        modalPanelObject.SetActive(false);
    }
}
