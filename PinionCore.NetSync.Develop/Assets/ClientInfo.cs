using UnityEngine;
namespace PinionCore.NetSync.Sample1
{

    public class ClientInfo : MonoBehaviour
    {
        public TMPro.TMP_Text Ping;
        public PinionCore.NetSync.Client Client;
        float _PingTime;
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            _PingTime += UnityEngine.Time.deltaTime;
            if (_PingTime < 3)
                return;
            _PingTime = 0;
            Ping.text = Client.Ping.ToString();
        }
    }

}