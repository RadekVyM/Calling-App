using System.ComponentModel;
using System.Threading.Tasks;

namespace CallingApp.Core
{
    public interface IBasePageViewModel : INotifyPropertyChanged
    {
        Task OnPageDisappearing();
        Task OnPageAppearing();
        Task OnPageCreated(params object[] parameters);
    }
}
