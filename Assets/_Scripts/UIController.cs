using UnityEngine;
using System.Collections;
using AC;

public class UIController : MonoBehaviour
{
    public GameObject mapPanel1;
    public GameObject mapPanel2;
    public GameObject welcomePanel;
    public GameObject MenuPanel;

    public string AgentName;
    public string CurrentMenu;


    //Remove before publish
    public bool menuIsOpen;

    void Update()
    {
        
    }

    public void openPanel(GameObject panelName)
    {
        if(!panelName.gameObject.activeSelf)
            panelName.gameObject.SetActive(true);
    }
}
