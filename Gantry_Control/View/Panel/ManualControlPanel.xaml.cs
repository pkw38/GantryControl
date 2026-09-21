using Gantry_Control.ViewModel;
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

namespace Gantry_Control.View.Panel
{
    /// <summary>
    /// Control.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ManualControlPanel : UserControl
    {
        public ManualControlPanel()
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
            if (sender is not Button button || button.DataContext is not ManualViewModel.MoveButton item) return;
            if (item.IsStop || DataContext is not ManualViewModel vm) return;

            if (vm.DirectionPressCommand.CanExecute(item.DirectionKey))
                vm.DirectionPressCommand.Execute(item.DirectionKey);
        }

        private void DirectionButton_PressEnd(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.DataContext is not ManualViewModel.MoveButton item) return;
            if (item.IsStop || DataContext is not ManualViewModel vm) return;

            vm.DirectionReleaseCommand.Execute(null);
        }
    }
}
