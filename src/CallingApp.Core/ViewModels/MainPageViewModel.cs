using System.Windows.Input;
using CallingApp.Core.Models;

namespace CallingApp.Core.ViewModels;

public class MainPageViewModel : BasePageViewModel
{
    Person? person;

    public Person? CurrentPerson
    {
        get => person;
        set
        {
            person = value;
            OnPropertyChanged(nameof(CurrentPerson));
        }
    }
    public IList<Person> People => [new Person { Image = "vianey.jpg", Name = "Vianey" }, new Person { Image = "rick.jpg", Name = "Rick" }];
    public IList<string> Images => ["mailchimp.jpg", "acnestudios.jpg", "wholefoods.jpg"];

    public ICommand CallPersonCommand { get; private init; }

    public MainPageViewModel()
    {
        CallPersonCommand = new RelayCommand(parameter => CurrentPerson = parameter as Person);
    }
}