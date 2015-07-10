


namespace MinskTrans.DesctopClient.Modelview
{
using System.ComponentModel;
using System.Runtime.CompilerServices;
#if !WINDOWS_PHONE_APP && !WINDOWS_UAP
	using MinskTrans.DesctopClient.Annotations;
#else
	using MinskTrans.Universal.Annotations;

#endif
	public class BaseModelView : INotifyPropertyChanged
	{
		private readonly TimeTableRepositoryBase context;
		//protected readonly ISettingsModelView settingsModelView;

		//public BaseModelView()
		//	: this(null)
		//{
		//}

		public BaseModelView(TimeTableRepositoryBase newContext)
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

		public TimeTableRepositoryBase Context
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
			if (handler == null)
				return;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}