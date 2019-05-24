using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SeeingSharpModelViewer
{
    public class ViewServiceHandler
    {
        private Dictionary<Type, Type> m_viewServices;
        private FrameworkElement m_viewServiceHost;

        public void Initialize(FrameworkElement viewServiceHost)
        {
            m_viewServiceHost = viewServiceHost;

            m_viewServices = new Dictionary<Type, Type>();
            m_viewServices[typeof(ICommonDialogsViewService)] = typeof(CommonDialogsViewService);

            Messenger.Default.Register<QueryForViewServiceMessage>(this, OnMessage_QueryForViewServiceMessage, keepTargetAlive: true);
        }

        private void OnMessage_QueryForViewServiceMessage(QueryForViewServiceMessage message)
        {
            if(m_viewServices.ContainsKey(message.ViewServiceType))
            {
                message.ViewService = Activator.CreateInstance(m_viewServices[message.ViewServiceType]) as IViewService;
                message.ViewService.SetViewServiceHost(m_viewServiceHost);
            }
        }
    }
}
