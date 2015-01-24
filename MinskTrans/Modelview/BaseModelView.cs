using System.ComponentModel;
using System.Runtime.CompilerServices;
using MinskTrans.DesctopClient.Annotations;


namespace MinskTrans.DesctopClient.Modelview
{
	public class BaseModelView : INotifyPropertyChanged
	{
		private readonly Context context;
		//protected readonly ISettingsModelView settingsModelView;

		public BaseModelView()
			: this(null)
		{
		}

		public BaseModelView(Context newContext)
		{
			context = newContext ?? new Context();
			
		}

		//public ISettingsModelView SettingsModelView
		//{
		//	get { return settingsModelView;}
		//}

		public Context Context
		{
			get { return context; }
		}


		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}