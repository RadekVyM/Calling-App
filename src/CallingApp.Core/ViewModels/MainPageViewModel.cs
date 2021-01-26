using System.Collections.Generic;
using System.Windows.Input;

namespace CallingApp.Core
{
    public class MainPageViewModel : BasePageViewModel
    {
        Person person;

        public Person CurrentPerson
        {
            get => person;
            set
            {
                person = value;
                OnPropertyChanged(nameof(CurrentPerson));
            }
        }
        public IList<Person> People => new List<Person> { new Person { Image = "vianey.jpg", Name = "Vianey" }, new Person { Image = "rick.jpg", Name = "Rick" } };
        public IList<string> Images => new List<string> { "mailchimp.jpg", "acnestudios.jpg", "wholefoods.jpg" };

        public ICommand CallPersonCommand { get; private set; }

        public MainPageViewModel()
        {
            CallPersonCommand = new RelayCommand(parameter => CurrentPerson = parameter as Person);
        }
    }
}
