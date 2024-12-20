using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using Wpf.Ui.Controls;

namespace WPF_KYJ_CustomControl.Controls
{

    public class AsyncCommandButton : Button
    {
        public static readonly DependencyProperty AsyncCommandProperty = DependencyProperty.Register("AsyncCommand", typeof(IAsyncRelayCommand), typeof(AsyncCommandButton), new PropertyMetadata(null));

        public static readonly DependencyProperty CancelCommandProperty = DependencyProperty.Register("CancelCommand", typeof(ICommand), typeof(AsyncCommandButton), new PropertyMetadata(null));

        public static readonly DependencyProperty IsRunningProperty = DependencyProperty.Register("IsRunning", typeof(bool), typeof(AsyncCommandButton), new PropertyMetadata(false));

        public static readonly DependencyProperty AsyncCommandContentProperty = DependencyProperty.Register("AsyncCommandContent", typeof(object), typeof(AsyncCommandButton), new PropertyMetadata(null));

        public static readonly DependencyProperty CancelCommandContentProperty = DependencyProperty.Register("CancelCommandContent", typeof(object), typeof(AsyncCommandButton), new PropertyMetadata(null));

        public static readonly DependencyProperty ProgressRingVisibilityProperty = DependencyProperty.Register("ProgressRingVisibility", typeof(Visibility), typeof(AsyncCommandButton), new PropertyMetadata(null));

        public static readonly DependencyProperty ProgressRingSizeProperty = DependencyProperty.Register("ProgressRingSize", typeof(double), typeof(AsyncCommandButton), new PropertyMetadata(20.0));

        public static readonly DependencyProperty ProgressRingColorProperty = DependencyProperty.Register("ProgressRingColor", typeof(Brush), typeof(AsyncCommandButton), new PropertyMetadata(Brushes.LightBlue));


        public IAsyncRelayCommand AsyncCommand
        {
            get { return (IAsyncRelayCommand)GetValue(AsyncCommandProperty); }
            set { SetValue(AsyncCommandProperty, value); }
        }

        public ICommand CancelCommand
        {
            get { return (ICommand)GetValue(CancelCommandProperty); }
            set { SetValue(CancelCommandProperty, value); }
        }

        public bool IsRunning
        {
            get { return (bool)GetValue(IsRunningProperty); }
            set { SetValue(IsRunningProperty, value); }
        }

        public object AsyncCommandContent
        {
            get { return (object)GetValue(AsyncCommandContentProperty); }
            set { SetValue(AsyncCommandContentProperty, value); }
        }

        public object CancelCommandContent
        {
            get { return (object)GetValue(CancelCommandContentProperty); }
            set { SetValue(CancelCommandProperty, value); }
        }

        public Visibility ProgressRingVisibility
        {
            get { return (Visibility)GetValue(ProgressRingVisibilityProperty); }
            set { SetValue(ProgressRingVisibilityProperty, value); }
        }

        public double ProgressRingSize
        {
            get { return (double)GetValue(ProgressRingSizeProperty); }
            set { SetValue(ProgressRingSizeProperty, value); }
        }

        public Brush ProgressRingColor
        {
            get { return (Brush)GetValue(ProgressRingColorProperty); }
            set { SetValue(ProgressRingColorProperty, value); }
        }





        public AsyncCommandButton()
        {
            Click += async (s, e) => await OnButtonClickAsync();
        }

        private async Task OnButtonClickAsync()
        {
            if (AsyncCommand != null && AsyncCommand.CanExecute(null))
            {
                IsRunning = true;
                try
                {
                    await AsyncCommand.ExecuteAsync(null);
                }
                finally
                {
                    IsRunning = false;
                }
            }

            // AsyncCommand.IsRunning
            else if (CancelCommand != null && CancelCommand.CanExecute(null))
            {
                try
                {
                    CancelCommand.Execute(null);
                }
                finally
                {
                    IsRunning = false;
                }
            }
        }


    }
}
