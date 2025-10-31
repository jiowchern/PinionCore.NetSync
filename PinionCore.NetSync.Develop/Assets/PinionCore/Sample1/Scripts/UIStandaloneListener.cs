using PinionCore.NetSync.Standalone;
using System;
using Unity.VisualScripting;
using UnityEngine;


namespace PinionCore.NetSync.Sample1
{
    public class UIStandaloneListener : MonoBehaviour
    {
        public UnityEngine.UI.Button Listening;
        public Listener Listener;
        public void Start()
        {
            Listening.onClick.AddListener(_Click);
        }

        private void _Click()
        {
            if (Listener.IsConnect())
            {
                Listener.Close();
            }
            else
            {
                Listener.Bind();
            }
        }

    }

}
