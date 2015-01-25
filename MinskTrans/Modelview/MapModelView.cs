


namespace MinskTrans.DesctopClient.Modelview
{
	class MapModelView: BaseModelView
	{
		private Stop currentStop;
		private Rout currentRout;

		public MapModelView(Context context)
			:base(context)
		{
			
		}

		public Stop CurrentStop
		{
			get { return currentStop; }
			set
			{
				if (Equals(value, currentStop)) return;
				currentStop = value;
				OnPropertyChanged();
			}
		}

		public Rout CurrentRout
		{
			get { return currentRout; }
			set
			{
				if (Equals(value, currentRout)) return;
				currentRout = value;
				OnPropertyChanged();
			}
		}

		
	}
}
