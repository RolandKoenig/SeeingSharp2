using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using PropertyTools.DataAnnotations;
using PropertyTools.Wpf;

namespace SeeingSharp.WpfSamples
{
    public class PropertyGridControlFactory : PropertyTools.Wpf.PropertyGridControlFactory
    {
        public override FrameworkElement CreateControl(PropertyItem property, PropertyControlFactoryOptions options)
        {
            if (property.ActualPropertyType.IsAssignableFrom(typeof(ICommand)))
            {
                property.HeaderPlacement = HeaderPlacement.Hidden;

                var result = new Button
                {
                    Content = property.DisplayName
                };
                result.SetBinding(ButtonBase.CommandProperty, property.CreateBinding());
                return result;
            }

            return base.CreateControl(property, options);
        }

        protected override FrameworkElement CreateDefaultControl(PropertyItem property)
        {
            property.AutoUpdateText = true;
            return base.CreateDefaultControl(property);
        }
    }
}