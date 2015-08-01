using UnityEngine;
using System.Collections;
using AC;

namespace AC
{
    public class EnableDisableGameObject : MonoBehaviour
    {

        public GameObject objectToControl;
        private ActionListManager _AC;


        void Start()
        {
            _AC = new ActionListManager();
        }

        void Update()
        {
            bool running = _AC.AreActionListsRunning();
            if (running)
                Debug.Log("actionList is running!");
          //  Debug.Log("is Running" + running);
        }

        public void turnOn()
        {
            this.gameObject.SetActive(true);
        }

        public void turnOff()
        {
            this.gameObject.SetActive(false);
        }
    }
}


