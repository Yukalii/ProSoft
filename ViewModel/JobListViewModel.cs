using EasySave.Model.Backup;

namespace EasySave.ViewModel
{
    public class JobEditorViewModel : ViewModelBase
    {
        private readonly BackupJobManager _jobManager;
        private readonly Action _onSaved;
        private readonly string? _originalName;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string SourcePath { get => _source; set => SetProperty(ref _source, value); }
        public string TargetPath { get => _target; set => SetProperty(ref _target, value); }
        public string SelectedStrategy { get => _strat; set => SetProperty(ref _strat, value); }

        private string _name = "", _source = "", _target = "", _strat = "FullBackupStrategy";

        public List<string> Strategies { get; } = new() { "FullBackupStrategy", "DifferentialBackupStrategy" };

        public string Title => _originalName == null ? "Nouveau job" : "Modifier le job";

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public JobEditorViewModel(BackupJobManager jobManager, Action onSaved)
        {
            _jobManager = jobManager;
            _onSaved = onSaved;

            SaveCommand = new RelayCommand(
                _ => { Save(); _onSaved(); },
                _ => !string.IsNullOrWhiteSpace(Name)
            );
            CancelCommand = new RelayCommand(_ => _onSaved());
        }

        public JobEditorViewModel(BackupJobManager jobManager, Action onSaved, BackupJob jobToEdit)
            : this(jobManager, onSaved)
        {
            _originalName = jobToEdit.Name;
            Name = jobToEdit.Name;
            SourcePath = jobToEdit.SourcePath;
            TargetPath = jobToEdit.TargetPath;
            SelectedStrategy = jobToEdit.StrategyName;
        }

        public void Save()
        {
            if (_originalName != null)
                _jobManager.DeleteJob(_originalName); 

            _jobManager.AddJob(Name, SourcePath, TargetPath, SelectedStrategy);

            Name = "";
            SourcePath = "";
            TargetPath = "";
            SelectedStrategy = "FullBackupStrategy";
        }
    }
}