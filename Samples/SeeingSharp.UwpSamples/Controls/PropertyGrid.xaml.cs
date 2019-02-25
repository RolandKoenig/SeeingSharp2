using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace SeeingSharp.UwpSamples.Controls
{
    public sealed partial class PropertyGrid : UserControl
    {
        public static readonly DependencyProperty SelectedObjectProperty =
            DependencyProperty.Register(nameof(PropertyGrid.SelectedObject), typeof(object), typeof(PropertyGrid), new PropertyMetadata(null, OnSelectedObjectChanged));

        private PropertyGridViewModel m_propertyGridVM;

        public PropertyGrid()
        {
            this.InitializeComponent();

            m_propertyGridVM = new PropertyGridViewModel();
            this.GridMain.DataContext = m_propertyGridVM;
        }

        private void UpdatePropertiesView()
        {
            GridMain.Children.Clear();

            var lstProperties = new List<ConfigurablePropertyMetadata>(m_propertyGridVM.PropertyMetadata);
            lstProperties.Sort((left, right) => left.CategoryName.CompareTo(right.CategoryName));
            var lstPropertyCategories = lstProperties
                .Select((actProperty) => actProperty.CategoryName)
                .Distinct()
                .ToList();

            // Define rows
            GridMain.RowDefinitions.Clear();
            int rowCount = lstProperties.Count + lstPropertyCategories.Count;
            for(int loop=0; loop<rowCount; loop++)
            {
                GridMain.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(45.0) });
            }
            this.Height = rowCount * 45d;

            // Create all controls
            int actRowIndex = 0;
            string actCategory = string.Empty;
            foreach(var actProperty in m_propertyGridVM.PropertyMetadata)
            {
                if(actProperty.CategoryName != actCategory)
                {
                    actCategory = actProperty.CategoryName;

                    TextBlock txtHeader = new TextBlock();
                    txtHeader.Text = actCategory;
                    txtHeader.SetValue(Grid.RowProperty, (double)actRowIndex);
                    txtHeader.SetValue(Grid.ColumnSpanProperty, 2d);
                    txtHeader.SetValue(Grid.ColumnProperty, 0d);
                    txtHeader.Margin = new Thickness(5d, 5d, 5d, 5d);
                    txtHeader.VerticalAlignment = VerticalAlignment.Bottom;
                    txtHeader.FontWeight = Windows.UI.Text.FontWeights.Bold;
                    GridMain.Children.Add(txtHeader);

                    Windows.UI.Xaml.Shapes.Rectangle rect = new Windows.UI.Xaml.Shapes.Rectangle();
                    rect.Height = 2d;
                    rect.Fill = new SolidColorBrush(Windows.UI.Colors.Black);
                    rect.VerticalAlignment = VerticalAlignment.Bottom;
                    rect.Margin = new Thickness(5d, 5d, 5d, 0d);
                    rect.SetValue(Grid.RowProperty, (double)actRowIndex);
                    rect.SetValue(Grid.ColumnSpanProperty, 2d);
                    rect.SetValue(Grid.ColumnProperty, 0d);
                    GridMain.Children.Add(rect);

                    actRowIndex++;
                }

                TextBlock ctrlText = new TextBlock();
                ctrlText.Text = actProperty.PropertyDisplayName;
                ctrlText.SetValue(Grid.RowProperty, (double)actRowIndex);
                ctrlText.SetValue(Grid.ColumnProperty, 0d);
                ctrlText.Margin = new Thickness(5d, 5d, 50d, 5d);
                ctrlText.VerticalAlignment = VerticalAlignment.Center;
                GridMain.Children.Add(ctrlText);

                FrameworkElement ctrlValueEdit = null;
                switch (actProperty.ValueType)
                {
                    case PropertyValueType.Bool:
                        CheckBox ctrlCheckBox = new CheckBox();
                        ctrlCheckBox.SetBinding(CheckBox.IsCheckedProperty, new Binding()
                        {
                            Path = new PropertyPath(nameof(actProperty.ValueAccessor)),
                            Mode = BindingMode.TwoWay,
                            Source = actProperty,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        });
                        ctrlValueEdit = ctrlCheckBox;
                        break;

                    case PropertyValueType.String:
                        TextBox ctrlTextBox = new TextBox();
                        ctrlTextBox.SetBinding(TextBox.TextProperty, new Binding()
                        {
                            Path = new PropertyPath(nameof(actProperty.ValueAccessor)),
                            Mode = BindingMode.TwoWay,
                            Source = actProperty,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        });
                        ctrlTextBox.Width = 200d;
                        ctrlValueEdit = ctrlTextBox;
                        break;

                    case PropertyValueType.Enum:
                        ComboBox ctrlComboBox = new ComboBox();
                        ctrlComboBox.ItemsSource = actProperty.GetEnumMembers();
                        ctrlComboBox.SetBinding(ComboBox.SelectedItemProperty, new Binding()
                        {
                            Path = new PropertyPath(nameof(actProperty.ValueAccessor)),
                            Mode = BindingMode.TwoWay,
                            Source = actProperty,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        });
                        ctrlComboBox.Width = 200d;
                        ctrlValueEdit = ctrlComboBox;
                        break;
                }

                if (ctrlValueEdit != null)
                {
                    ctrlValueEdit.Margin = new Thickness(0d, 0d, 5d, 0d);
                    ctrlValueEdit.VerticalAlignment = VerticalAlignment.Center;
                    ctrlValueEdit.SetValue(Grid.RowProperty, (double)actRowIndex);
                    ctrlValueEdit.SetValue(Grid.ColumnProperty, 1d);
                    GridMain.Children.Add(ctrlValueEdit);
                }

                actRowIndex++;
            }
        }

        private static void OnSelectedObjectChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eArgs)
        {
            if(!(sender is PropertyGrid propGrid)) { return; }

            propGrid.m_propertyGridVM.SelectedObject = eArgs.NewValue;
            propGrid.UpdatePropertiesView();
        }


        public object SelectedObject
        {
            get { return (object)GetValue(SelectedObjectProperty); }
            set { SetValue(SelectedObjectProperty, value); }
        }
    }
}
