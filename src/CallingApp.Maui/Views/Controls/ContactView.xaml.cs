namespace CallingApp.Maui.Views.Controls;

public partial class ContactView : Grid
{
	public event EventHandler CallButtonClicked { add => callButton.Clicked += value; remove => callButton.Clicked -= value; }

	public ContactView()
	{
		InitializeComponent();
	}
}