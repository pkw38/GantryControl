using Gantry_Control.ViewModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gantry_Control.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        protected override void OnDeactivated(EventArgs e)
        {
            // Alt+Tab 등으로 포커스를 잃으면 버튼 Release를 받을 수 없으므로 정지
            (DataContext as MainViewModel)?.StopMotion();
            base.OnDeactivated(e);
        }
    }
}