#nullable disable

using System.ComponentModel;
using System.Reactive.Disposables;
using Kyyla.ViewModel;
using ReactiveUI;

namespace Kyyla
{
    /// <summary>
    /// Interaction logic for AbsenceWindow.xaml
    /// </summary>
    public partial class AbsenceWindow : ReactiveWindow<AbsenceViewModel>
    {
        public AbsenceWindow()
        {
            InitializeComponent();
            ViewModel = new AbsenceViewModel();
            
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel, vm => vm.Rows, v => v.AbsenceListView.ItemsSource)
                    .DisposeWith(disposables);
            });
        }
    }
}
