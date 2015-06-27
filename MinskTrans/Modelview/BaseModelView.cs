

using MinskTrans.Universal.Annotations;

namespace MinskTrans.DesctopClient.Modelview
{
using System.ComponentModel;
using System.Runtime.CompilerServices;
#if !WINDOWS_PHONE_APP
using MinskTrans.DesctopClient.Annotations;
#else

#endif
	public class BaseModelView : INotifyPropertyChanged
	{
		private readonly Context context;
		//protected readonly ISettingsModelView settingsModelView;

		//public BaseModelView()
		//	: this(null)
		//{
		//}

		public BaseModelView(Context newContext)
		{
			context = newContext;
			
		}

		public virtual void RefreshView()
		{
			
		}

		//public ISettingsModelView SettingsModelView
		//{
		//	get { return settingsModelView;}
		//}

		public Context Context
		{
			get { return context; }
		}

		public virtual void Refresh()
		{
			
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