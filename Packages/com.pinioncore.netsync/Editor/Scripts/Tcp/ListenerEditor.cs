﻿using PinionCore.NetSync.Extensions;
using PinionCore.NetSync.Tcp;
using UnityEditor;
using UnityEngine.UIElements;

namespace PinionCore.NetSync.Editor.Tcp
{
    [CustomEditor(typeof(PinionCore.NetSync.Tcp.TcpListener))]

    public class ListenerEditor : UnityEditor.Editor
    {
        private TcpListener _Target;

        private void OnEnable()
        {
            _Target = (PinionCore.NetSync.Tcp.TcpListener)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var element = new VisualElement();
            var root = EditorGUIUtility.Load("Packages/com.pinioncore.netsync/Editor/Resources/Layouts/Tcp/Listener.uxml") as VisualTreeAsset;
            if (root == null)
                return base.CreateInspectorGUI();
            root.CloneTree(element);

            var status = element.Q<Label>("Status");
            status.SetTextBinding(_Target, nameof(_Target.CurrentStatus), BindingMode.ToTarget);

            var send = element.Q<Label>("Send");
            send.SetTextBinding(_Target, nameof(_Target.BytesSent), BindingMode.ToTarget);

            var receive = element.Q<Label>("Receive");
            receive.SetTextBinding(_Target, nameof(_Target.BytesReceived), BindingMode.ToTarget);

            return element;
        }
    }

}
