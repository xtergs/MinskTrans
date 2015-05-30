using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MinskTrans.DesctopClient.Model;
using MinskTrans.DesctopClient.Modelview;


namespace MinskTrans.DesctopClient
{
	/// <summary>
	/// Interaction logic for GroupAddWindow.xaml
	/// </summary>
	public partial class GroupAddWindow : Window
	{
		readonly GroupEditModelView groupEditModelView;
		public GroupAddWindow(Context newContext)
		{
			InitializeComponent();
			groupEditModelView = new GroupEditModelView(newContext, null);
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
