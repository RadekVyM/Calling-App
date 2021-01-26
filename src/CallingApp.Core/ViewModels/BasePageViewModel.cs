using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace CallingApp.Core
{
    public class BasePageViewModel : IBasePageViewModel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            try
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
            }
        }

        public async virtual Task OnPageDisappearing()
        {
            await Task.CompletedTask;
        }

        public async virtual Task OnPageCreated(params object[] parameters)
        {
            await Task.CompletedTask;
        }

        public async virtual Task OnPageAppearing()
        {
            await Task.CompletedTask;
        }
    }
}
