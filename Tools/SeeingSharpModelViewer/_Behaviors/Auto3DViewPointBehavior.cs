using GalaSoft.MvvmLight.Messaging;
using SeeingSharp;
using SeeingSharp.Multimedia.Views;
using SeeingSharp.Util;
using System;
using System.Windows.Interactivity;

namespace SeeingSharpModelViewer
{
    public class Auto3DViewPointBehavior : Behavior<SeeingSharpRendererElement>
    {
        private IDisposable m_subscription;

        protected override void OnAttached()
        {
            base.OnAttached();

            Messenger.Default.Register<NewModelLoadedMessage>(this, this.OnMessage_NewModelLoadedMessage);

            this.AssociatedObject.Loaded += this.OnAssociatedObject_Loaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            this.AssociatedObject.Loaded -= this.OnAssociatedObject_Loaded;
            SeeingSharpUtil.SafeDispose(ref m_subscription);
        }

        private async void ApplyNewCameraLocation()
        {
            if (this.AssociatedObject == null) { return; }

            await this.AssociatedObject.RenderLoop.WaitForNextFinishedRenderAsync();

            await this.AssociatedObject.RenderLoop.MoveCameraToDefaultLocationAsync(
                EngineMath.RAD_45DEG, EngineMath.RAD_45DEG);
        }

        private void OnAssociatedObject_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.ApplyNewCameraLocation();
        }

        private void OnMessage_NewModelLoadedMessage(NewModelLoadedMessage message)
        {
            this.ApplyNewCameraLocation();
        }
    }
}