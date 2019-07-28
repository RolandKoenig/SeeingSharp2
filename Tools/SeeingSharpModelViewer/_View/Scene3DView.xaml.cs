using System.Windows.Controls;
using System.Collections.Generic;
using SeeingSharp.Multimedia.Components;

namespace SeeingSharpModelViewer
{
    /// <summary>
    /// Interaction logic for Scene3DView.xaml
    /// </summary>
    public partial class Scene3DView : UserControl
    {
        public Scene3DView()
        {
            this.InitializeComponent();

            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            CtrlRenderer.RenderLoop.SceneComponents.Add(
                new FreeMovingCameraComponent());
        }
    }
}