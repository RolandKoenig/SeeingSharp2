using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace SeeingSharpModelViewer
{
    public class ModelViewerIconExtension : MarkupExtension
    {
        private static Dictionary<string, BitmapImage> s_images;

        /// <summary>
        /// Initializes the <see cref="ModelViewerIconExtension"/> class.
        /// </summary>
        static ModelViewerIconExtension()
        {
            s_images = new Dictionary<string, BitmapImage>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelViewerIconExtension"/> class.
        /// </summary>
        public ModelViewerIconExtension()
        {
            this.Icon = ModelViewerIcon.Open;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var uriPath = $"pack://application:,,,/SeeingSharpModelViewer;component/Assets/Icons/{this.Icon}_16x16.png";

            // Load the ImageSource (me may have cached it before
            BitmapImage result = null;
            void ActionCreateBitmapSource()
            {
                if (!s_images.TryGetValue(uriPath, out result))
                {
                    result = new BitmapImage(new Uri(uriPath, UriKind.Absolute));
                    result.Freeze();
                    s_images.Add(uriPath, result);
                }
            }

            // Create the result object
            switch (this.ResultType)
            {
                case IconResultType.Image:
                    ActionCreateBitmapSource();
                    var imgControl = new Image();
                    imgControl.Source = result;
                    imgControl.Width = 16.0;
                    imgControl.Height = 16.0;
                    return imgControl;

                case IconResultType.BitmapImage:
                    ActionCreateBitmapSource();
                    return result;
            }

            return null;
        }

        public ModelViewerIcon Icon
        {
            get;
            set;
        }

        public IconResultType ResultType
        {
            get;
            set;
        }
    }
}