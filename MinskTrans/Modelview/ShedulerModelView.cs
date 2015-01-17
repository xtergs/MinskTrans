using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using MinskTrans.Annotations;

namespace MinskTrans
{
	public class ShedulerModelView : INotifyPropertyChanged
	{
		private readonly Context context;

		public Context Context
		{
			get { return context; }
		}

		public ShedulerModelView()
			:this(null)
		{
		}

		public ShedulerModelView(Context newContext)
		{
			context = newContext ?? new Context();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
