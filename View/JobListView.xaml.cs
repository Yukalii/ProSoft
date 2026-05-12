namespace EasySave.View
{
    public partial class JobListView : UserControl
    {
        public JobListView()
        {
            InitializeComponent();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.DataContext is ViewModel.JobListItemViewModel item)
            {
                if (DataContext is ViewModel.JobListViewModel vm)
                {
                    vm.ToggleJobSelection(item, cb.IsChecked == true);
                }
            }
        }
    }
}