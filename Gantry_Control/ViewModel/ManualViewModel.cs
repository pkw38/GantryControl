using CommunityToolkit.Mvvm.ComponentModel;
using Gantry_Control.ViewModel.Tab.ManualTab;
using System.Collections.ObjectModel;

namespace Gantry_Control.ViewModel
{
    public partial class ManualViewModel : ObservableObject
    {
        public ObservableCollection<ManualTabViewModel> Tabs { get; } = new()
        {
            new JogTabViewModel(),
        };

        [ObservableProperty]
        private ManualTabViewModel? selectedTab;

        public ManualViewModel()
        {
            SelectedTab = Tabs[0];
        }

        public void StopMotion()
        {
            foreach (var tab in Tabs)
            {
                tab.StopMotion();
            }
        }
    }
}
