using System.Windows.Input;
using CallingApp.Core.Models;

namespace CallingApp.Core.ViewModels;

public class MainPageViewModel : BasePageViewModel
{
    Contact? person;

    public Contact? CurrentPerson
    {
        get => person;
        set
        {
            person = value;
            OnPropertyChanged(nameof(CurrentPerson));
        }
    }
    public IList<Contact> CloseFriends =>
    [
        new Contact { Image = "vianey.jpg", Name = "Vianey" },
        new Contact { Image = "skyler.jpg", Name = "Skyler" },
        new Contact { Image = "rick.jpg", Name = "Rick" },
    ];
    public IList<ContactGroup> Contacts =>
    [
        new ContactGroup
        {
            Title = "A",
            Contacts =
            [
                new Contact { Image = "blank.jpg", Name = "Adriel" },
                new Contact { Image = "ariel.jpg", Name = "Ariel" },
                new Contact { Image = "arthur.jpg", Name = "Arthur" },
            ]
        },
        new ContactGroup
        {
            Title = "C",
            Contacts =
            [
                new Contact { Image = "cory.jpg", Name = "Cory" },
            ]
        },
        new ContactGroup
        {
            Title = "E",
            Contacts =
            [
                new Contact { Image = "elsa.jpg", Name = "Elsa" },
            ]
        },
        new ContactGroup
        {
            Title = "J",
            Contacts =
            [
                new Contact { Image = "jemma.jpg", Name = "Jemma" },
                new Contact { Image = "john.jpg", Name = "John" },
            ]
        },
        new ContactGroup
        {
            Title = "K",
            Contacts =
            [
                new Contact { Image = "kevin.jpg", Name = "Kevin" },
            ]
        },
        new ContactGroup
        {
            Title = "L",
            Contacts =
            [
                new Contact { Image = "blank.jpg", Name = "Lola" },
            ]
        },
        new ContactGroup
        {
            Title = "M",
            Contacts =
            [
                new Contact { Image = "blank.jpg", Name = "Murphy" },
            ]
        },
        new ContactGroup
        {
            Title = "R",
            Contacts =
            [
                new Contact { Image = "rick.jpg", Name = "Rick" },
            ]
        },
        new ContactGroup
        {
            Title = "S",
            Contacts =
            [
                new Contact { Image = "skyler.jpg", Name = "Skyler" },
            ]
        },
        new ContactGroup
        {
            Title = "V",
            Contacts =
            [
                new Contact { Image = "vianey.jpg", Name = "Vianey" },
            ]
        },
    ];

    public ICommand CallPersonCommand { get; private init; }

    public MainPageViewModel()
    {
        CallPersonCommand = new RelayCommand(parameter => CurrentPerson = parameter as Contact);
    }
}