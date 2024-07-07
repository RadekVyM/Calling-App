using System.ComponentModel;

namespace CallingApp.Core.ViewModels;

public interface IBasePageViewModel : INotifyPropertyChanged
{
    Task OnPageDisappearing();
    Task OnPageAppearing();
    Task OnPageCreated(params object[] parameters);
}