using System.Windows.Input;

namespace CallingApp.Core;

public class RelayCommand : ICommand
{
    private readonly Action<object?>? parameteredAction;
    private readonly Action? action;
    private readonly Predicate<object?>? canExecute;

    public event EventHandler? CanExecuteChanged;

    public RelayCommand(Action action, Predicate<object?>? canExecute)
    {
        this.action = action;
        this.canExecute = canExecute;
    }

    public RelayCommand(Action<object?> parameteredAction, Predicate<object?>? canExecute)
    {
        this.parameteredAction = parameteredAction;
        this.canExecute = canExecute;
    }

    public RelayCommand(Action action) : this(action, null) { }

    public RelayCommand(Action<object?> parameteredAction) : this(parameteredAction, null) { }

    public bool CanExecute(object? parameter) =>
        canExecute is null || canExecute(parameter);

    public void Execute(object? parameter)
    {
        parameteredAction?.Invoke(parameter);
        action?.Invoke();
    }

    public void OnCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}