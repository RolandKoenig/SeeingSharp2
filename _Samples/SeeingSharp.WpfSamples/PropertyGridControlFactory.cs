using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PropertyTools.DataAnnotations;
using PropertyTools.Wpf;

namespace SeeingSharp.WpfSamples
{
    public class PropertyGridControlFactory : PropertyTools.Wpf.PropertyGridControlFactory
    {
        public override FrameworkElement CreateControl(PropertyItem property, PropertyControlFactoryOptions options)
        {
            if(property.ActualPropertyType.IsAssignableFrom(typeof(ICommand)))
            {
                property.HeaderPlacement = HeaderPlacement.Hidden;

                var result = new Button()
                {
                    Content = property.DisplayName,
                };
                result.SetBinding(Button.CommandProperty, property.CreateBinding());
                return result;
            }

            return base.CreateControl(property, options);
        }
    }
}
