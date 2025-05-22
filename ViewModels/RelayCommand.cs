using System;
using System.Windows.Input;

namespace ArtGalleryStore.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            bool result = _canExecute == null || _canExecute(parameter);
            System.Diagnostics.Debug.WriteLine($"RelayCommand.CanExecute: {result} (Command={this.GetHashCode()}, Parameter={parameter?.GetType().Name ?? "null"})");
            return result;
        }

        public void Execute(object? parameter)
        {
            System.Diagnostics.Debug.WriteLine($"RelayCommand.Execute: Command={this.GetHashCode()}, Parameter={parameter?.GetType().Name ?? "null"}");
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}