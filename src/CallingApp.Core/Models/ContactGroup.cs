namespace CallingApp.Core.Models;

public class ContactGroup
{
    public required string Title { get; init; }
    public required IReadOnlyList<Contact> Contacts { get; init; }
}