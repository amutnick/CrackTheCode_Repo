using UnityEngine;
using System.Collections;
using AC;
using UnityEngine.UI;

public class PassVar : MonoBehaviour {
    public Text agentName;
    public Text inputText;

    void Start () 
    {
        Debug.Log("Agent Name Var: " + AC.GlobalVariables.GetVariable(1).label);
        Debug.Log("Agent Name Value: " + AC.GlobalVariables.GetVariable(1).val.ToString());
        agentName = GameObject.Find("InputText").GetComponent<Text>();
    }

    void FixedUpdate()
    {
      //  Debug.Log(agentName.text);

    }

    public void SetString()
    {
       // Name = GetComponent<InputField>().text;
        Debug.Log("Current Value is: " + agentName.text);

    }

    public void changeVar()
    {
        AC.GlobalVariables.SetStringValue(1, agentName.text);
    }

}
