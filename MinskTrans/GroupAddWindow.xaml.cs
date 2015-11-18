using System.Windows;
using System.Windows.Controls;
using MinskTrans.Context;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.DesctopClient.Modelview;
using MyLibrary;


namespace MinskTrans.DesctopClient
{
	/// <summary>
	/// Interaction logic for GroupAddWindow.xaml
	/// </summary>
	public partial class GroupAddWindow : Window
	{
		readonly GroupEditModelView groupEditModelView;
		public GroupAddWindow(IBussnessLogics newContext, ISettingsModelView settings)
		{
			InitializeComponent();
			groupEditModelView = new GroupEditModelView(newContext, settings);
			DataContext = groupEditModelView;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		public GroupStop Group
		{
			get { return groupEditModelView.Stop; }
			set { groupEditModelView.Stop = new GroupStop(value); }
		}

		private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(((TextBox) sender).Text))
				saveButton.IsEnabled = false;
			else
			{
				saveButton.IsEnabled = true;
			}

		}
	}
}
