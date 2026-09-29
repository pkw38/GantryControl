using Gantry_Control.ViewModel.Tab.ManualTab;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gantry_Control.View.Tab.ManualTab
{
    /// <summary>
    /// JogTab.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class JogTab : UserControl
    {
        public JogTab()
        {
            InitializeComponent();
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button button && button.Template.FindName("border", button) is Border border)
            {
                ShadowAssist.SetDarken(border, false);
            }
        }


        private void DirectionButton_PressStart(object sender, RoutedEventArgs e)
        {
            // 좌클릭/터치만 조그 시작 (우클릭, 휠클릭 무시)
            if (e is MouseButtonEventArgs mouse && mouse.ChangedButton != MouseButton.Left) return;
            if (sender is not Button button || button.DataContext is not JogTabViewModel.MoveButton item) return;
            if (item.IsStop || DataContext is not JogTabViewModel vm) return;

            if (vm.DirectionPressCommand.CanExecute(item.DirectionKey))
                vm.DirectionPressCommand.Execute(item.DirectionKey);
        }

        private void DirectionButton_PressEnd(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.DataContext is not JogTabViewModel.MoveButton item) return;
            if (item.IsStop || DataContext is not JogTabViewModel vm) return;

            vm.DirectionReleaseCommand.Execute(null);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            (DataContext as JogTabViewModel)?.StopMotion();
        }
    }
}
