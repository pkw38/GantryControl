using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gantry_Control.Service;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;

namespace Gantry_Control.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {
        public class PanelItem
        {
            public string Page { get; }
            public string Label { get; }

            public PanelItem(string page, string label)
            {
                Page = page;
                Label = label;
            }
        }

        [ObservableProperty]
        private string selectedPanel = "home";
        [ObservableProperty]
        private object? currentViewModel;

        public ObservableCollection<PanelItem> PanelItems { get; } = new()
        {
            new PanelItem("home", "Home"),
            new PanelItem("history", "History"),
            new PanelItem("manual", "Manual Control"),
            new PanelItem("monitoring", "Monitoring"),
            new PanelItem("test", "Test")
        };

        public MainViewModel()
        {
            CurrentViewModel = new HomeViewModel();
        }

        [RelayCommand]
        private void ChangePanel(string page)
        {
            SelectedPanel = page;

            CurrentViewModel = page switch
            {
                "home" => new HomeViewModel(),
                "history" => new HistoryViewModel(),
                "manual" => new ManualViewModel(),
                "monitoring" => new MonitoringViewModel(),
                "test" => new TestViewModel(),
                _ => new HomeViewModel()
            };
        }
    }

    public class HomeViewModel
    {
    }

    public class HistoryViewModel
    {
    }

    public class MonitoringViewModel
    {
    }

    public class TestViewModel
    {
    }
}
