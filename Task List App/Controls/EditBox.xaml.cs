using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Task_List_App.Controls
{
    public sealed partial class EditBox : UserControl
    {
        static Windows.UI.Color _color = Windows.UI.Color.FromArgb(255, 200, 200, 200);

        #region [Properties]
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(EditBox),
            new PropertyMetadata("", OnTitleChanged));
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            nameof(Data),
            typeof(string),
            typeof(EditBox),
            new PropertyMetadata("", OnDataChanged));
        public string Data
        {
            get { return (string)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(EditBox),
            new PropertyMetadata(true, OnIsReadOnlyChanged));
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public static new readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register(
            nameof(BorderBrush),
            typeof(Brush),
            typeof(SeparatorLine),
         new PropertyMetadata(new SolidColorBrush(_color)));

        public new Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        #endregion

        public EditBox()
        {
            this.InitializeComponent();
            this.Loaded += EditBox_Loaded;
            this.Tapped += EditBox_Tapped;
            this.LostFocus += EditBox_LostFocus;
            this.PointerEntered += EditBox_PointerEntered;
            this.PointerExited += EditBox_PointerExited;
        }

        void EditBox_Tapped(object sender, TappedRoutedEventArgs e) => ChangeStates();
        void EditBox_PointerEntered(object sender, PointerRoutedEventArgs e) => this.ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
        void EditBox_PointerExited(object sender, PointerRoutedEventArgs e) => this.ProtectedCursor = null;

        void EditBox_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"[INFO] Loaded {sender.GetType().Name} of base type {sender.GetType().BaseType?.Name}");
        }

        void EditBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"[INFO] {sender.GetType().Name} LostFocus");
            tbData.IsHitTestVisible = false;
            IsReadOnly = true;
        }

        void ChangeStates()
        {
            if (IsReadOnly)
            {
                IsReadOnly = false;
                tbData.IsHitTestVisible = true;
                tbData.Focus(FocusState.Programmatic);
                tbData.SelectAll();
            }
            else
            {
                tbData.IsHitTestVisible = false;
                IsReadOnly = true;
            }
        }


        static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            //Debug.WriteLine($"[OnDataChanged]");
            var self = (EditBox)d;
            //var color = self.Background as SolidColorBrush;
            //color.Color =  Windows.UI.Color.FromArgb(255, (byte)args.NewValue, (byte)args.NewValue, (byte)args.NewValue);
            //self.Background = color;
        }
        static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            //Debug.WriteLine($"[OnTitleChanged]");
            var self = (EditBox)d;
            //var color = self.Background as SolidColorBrush;
            //color.Color =  Windows.UI.Color.FromArgb(255, (byte)args.NewValue, (byte)args.NewValue, (byte)args.NewValue);
            //self.Background = color;
        }
        static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            //Debug.WriteLine($"[OnIsReadOnlyChanged]");
            var self = (EditBox)d;
            //var color = self.Background as SolidColorBrush;
            //color.Color =  Windows.UI.Color.FromArgb(255, (byte)args.NewValue, (byte)args.NewValue, (byte)args.NewValue);
            //self.Background = color;
        }

    }
}
