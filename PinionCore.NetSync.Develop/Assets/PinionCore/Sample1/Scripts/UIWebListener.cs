using UnityEngine;
namespace PinionCore.NetSync.Sample1
{
    public class UIWebListener : MonoBehaviour
    {
        public TMPro.TMP_InputField Bind;
        public Web.WebListener Listener;
        public UnityEngine.UI.Button StartListening;
        public void Start()
        {
            StartListening.onClick.AddListener(_Click);
        }

        public void OnDestroy()
        {
            StartListening.onClick.RemoveListener(_Click);

        }

        private void _Click()
        {
            if (!Listener.IsListening)
            {
                

                Listener.Bind(int.Parse( Bind.text));
            }
            else
            {
                Listener.Close();
            }
        }
    }

}