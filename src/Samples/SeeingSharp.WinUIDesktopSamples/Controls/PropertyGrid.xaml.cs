using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Shapes;

namespace SeeingSharp.WinUIDesktopSamples.Controls
{
    public sealed partial class PropertyGrid : UserControl
    {
        public static readonly DependencyProperty SelectedObjectProperty =
            DependencyProperty.Register(nameof(SelectedObject), typeof(object), typeof(PropertyGrid), new PropertyMetadata(null, OnSelectedObjectChanged));

        private PropertyGridViewModel _propertyGridVM;

        public object SelectedObject
        {
            get => this.GetValue(SelectedObjectProperty);
            set => this.SetValue(SelectedObjectProperty, value);
        }

        public PropertyGrid()
        {
            this.InitializeComponent();

            _propertyGridVM = new PropertyGridViewModel();
            GridMain.DataContext = _propertyGridVM;
        }

        private static void OnSelectedObjectChanged(DependencyObject sender, DependencyPropertyChangedEventArgs eArgs)
        {
            if (!(sender is PropertyGrid propGrid)) { return; }

            propGrid._propertyGridVM.SelectedObject = eArgs.NewValue;
            propGrid.UpdatePropertiesView();
        }

        private void UpdatePropertiesView()
        {
            GridMain.Children.Clear();

            var lstProperties = new List<ConfigurablePropertyMetadata>(_propertyGridVM.PropertyMetadata);
            lstProperties.Sort((left, right) => string.Compare(left.CategoryName, right.CategoryName, StringComparison.Ordinal));
            var lstPropertyCategories = lstProperties
                .Select(actProperty => actProperty.CategoryName)
                .Distinct()
                .ToList();

            // Define rows
            GridMain.RowDefinitions.Clear();
            var rowCount = lstProperties.Count + lstPropertyCategories.Count;
            for (var loop = 0; loop < rowCount; loop++)
            {
                GridMain.RowDefinitions.Add(new RowDefinition { Height = new GridLength(38.0) });
            }
            this.Height = rowCount * 38d;

            // Create all controls
            var actRowIndex = 0;
            var actCategory = string.Empty;
            foreach (var actProperty in _propertyGridVM.PropertyMetadata)
            {
                if (actProperty.CategoryName != actCategory)
                {
                    actCategory = actProperty.CategoryName;

                    var txtHeader = new TextBlock
                    {
                        Text = actCategory
                    };

                    txtHeader.SetValue(Grid.RowProperty, (double)actRowIndex);
                    txtHeader.SetValue(Grid.ColumnSpanProperty, 2d);
                    txtHeader.SetValue(Grid.ColumnProperty, 0d);
                    txtHeader.Margin = new Thickness(5d, 5d, 5d, 5d);
                    txtHeader.VerticalAlignment = VerticalAlignment.Bottom;
                    txtHeader.FontWeight = FontWeights.Bold;
                    GridMain.Children.Add(txtHeader);

                    var rect = new Rectangle
                    {
                        Height = 2d,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Margin = new Thickness(5d, 5d, 5d, 0d)
                    };

                    rect.SetValue(Grid.RowProperty, (double)actRowIndex);
                    rect.SetValue(Grid.ColumnSpanProperty, 2d);
                    rect.SetValue(Grid.ColumnProperty, 0d);
                    GridMain.Children.Add(rect);

                    actRowIndex++;
                }

                var ctrlText = new TextBlock
                {
                    Text = actProperty.PropertyDisplayName
                };

                ctrlText.SetValue(Grid.RowProperty, (double)actRowIndex);
                ctrlText.SetValue(Grid.ColumnProperty, 0d);
                ctrlText.Margin = new Thickness(5d, 5d, 50d, 5d);
                ctrlText.VerticalAlignment = VerticalAlignment.Center;
                GridMain.Children.Add(ctrlText);

                FrameworkElement ctrlValueEdit = null;

                switch (actProperty.ValueType)
                {
                    case PropertyValueType.Bool:
                        var ctrlCheckBox = new CheckBox();

                        ctrlCheckBox.SetBinding(ToggleButton.IsCheckedProperty, new Binding
                        {
                            Path = new PropertyPath(nameof(actProperty.ValueAccessor)),
                            Mode = BindingMode.TwoWay,
                            Source = actProperty,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        });

                        ctrlValueEdit = ctrlCheckBox;
                        break;

                    case PropertyValueType.String:
                        var ctrlTextBox = new TextBox();
                        ctrlTextBox.SetBinding(TextBox.TextProperty, new Binding
                        {
                            Path = new PropertyPath(nameof(actProperty.ValueAccessor)),
                            Mode = BindingMode.TwoWay,
                            Source = actProperty,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        });
                        ctrlTextBox.Width = 200d;
                        ctrlValueEdit = ctrlTextBox;
                        break;

                    case PropertyValueType.Enum:
                        var ctrlComboBox = new ComboBox();
                        ctrlComboBox.ItemsSource = actProperty.GetEnumMembers();
                        ctrlComboBox.SetBinding(Selector.SelectedItemProperty, new Binding
                        {
                            Path = new PropertyPath(nameof(actProperty.ValueAccessor)),
                            Mode = BindingMode.TwoWay,
                            Source = actProperty,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
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
    }
}
