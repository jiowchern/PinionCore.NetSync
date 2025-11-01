using PinionCore.Consoles.Chat1.Common;
using PinionCore.Network;
using PinionCore.Remote.Gateway;
using PinionCore.Remote.Ghost;
using PinionCore.Utility;
using System;
using Unity.Properties;


namespace PinionCore.NetSync.Samples.Chat
{
    internal class LoopState : IStatus
    {
        private readonly Client client;
        readonly private IAgent agent;
        readonly private IStreamable stream;
        

        
        public event Action<float> OnPingChanged;
        readonly TimeCounter timeCounter = new TimeCounter();

        public LoopState(Client client,IAgent agent, IStreamable stream)
        {
            this.client = client;
            this.agent = agent;
            this.stream = stream;
        }

      

        void IStatus.Update()
        {
            if(timeCounter.Second > 1)
            {
                timeCounter.Reset();
                OnPingChanged?.Invoke(agent.Ping);
            }            
            agent.HandleMessage();
            agent.HandlePackets();
            
        }


        void IStatus.Enter()
        {
            agent.Enable(stream);

            agent.QueryNotifier<ILogin>().Supply += _LoginSupply;
            agent.QueryNotifier<ILogin>().Unsupply += _LoginUnsupply;
            agent.QueryNotifier<IPlayer>().Supply += _PlayerSupply;
            agent.QueryNotifier<IPlayer>().Unsupply += _PlayerUnsupply;

        }
        void IStatus.Leave()
        {
            agent.Disable();

            agent.QueryNotifier<ILogin>().Supply -= _LoginSupply;
            agent.QueryNotifier<ILogin>().Unsupply -= _LoginUnsupply;
            agent.QueryNotifier<IPlayer>().Supply -= _PlayerSupply;
            agent.QueryNotifier<IPlayer>().Unsupply -= _PlayerUnsupply;
        }

        private void _PlayerUnsupply(IPlayer player)
        {
            client.OnPlayerUnsupply?.Invoke(player);
        }

        private void _LoginUnsupply(ILogin login)
        {
            client.OnLoginUnsupply?.Invoke(login);
        }

        private void _PlayerSupply(IPlayer player)
        {
            client.OnPlayerSupply?.Invoke(player);
        }

        private void _LoginSupply(ILogin login)
        {
            client.OnLoginSupply?.Invoke(login);
        }

    }
}