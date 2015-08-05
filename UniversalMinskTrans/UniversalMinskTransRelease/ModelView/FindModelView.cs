using GalaSoft.MvvmLight.Command;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Modelview;

namespace MinskTrans.Universal.ModelView
{
	public class FindModelView : BaseModelView
	{
	    public bool IsShowStopsView
	    {
	        get { return isShowStopsView; }
	        set
	        {
	            isShowStopsView = value;
	            OnPropertyChanged();
	        }
	    }

	    private readonly StopModelView stopModelView;
		private readonly RoutsModelView routsModelview;

		public StopModelView StopModelView
		{
			get
			{
				return stopModelView;
			}
		}

		public RoutsModelView RoutsModelView
		{
			get { return routsModelview;
			}
		}

	    private SettingsModelView mainSettings;
	    private bool isShowStopsView;
	    public SettingsModelView MainSettings { get { return mainSettings; } private set { mainSettings = value; } }

        public FindModelView(IContext newContext, SettingsModelView settingsModelView, bool UseGps = false) : base(newContext)
        {
            mainSettings = settingsModelView;
			stopModelView = new StopModelView(newContext, settingsModelView, UseGps);
			routsModelview = new RoutsModelView(newContext);
		}

	    private RelayCommand toggleFindViewCommanBack;

	    public RelayCommand ToggleFindViewCommand
	    {
	        get
	        {
	            if (toggleFindViewCommanBack == null)
	                toggleFindViewCommanBack = new RelayCommand(() =>
	                {
	                    IsShowStopsView = !IsShowStopsView;
	                    ToggleFindViewCommand.RaiseCanExecuteChanged();
	                }, () => !IsShowStopsView);
	            return toggleFindViewCommanBack;
	        }
	    }
	}
}
