using System.ComponentModel;
using System.Runtime.CompilerServices;
using MinskTrans.Library;
using MinskTrans.Library.Annotations;

namespace MinskTrans.DesctopClient.Modelview
{
	public class BaseModelView : INotifyPropertyChanged
	{
		private readonly Context context;

		public BaseModelView()
			: this(null)
		{
		}

		public BaseModelView(Context newContext)
		{
			context = newContext ?? new Context();
		}

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