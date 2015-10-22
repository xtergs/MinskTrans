using GalaSoft.MvvmLight.Command;
using MinskTrans.Context.Base;
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

	    private bool isShowTransportView;
        public bool IsShowTransportsView
        {
            get { return isShowTransportView; }
            set
            {
                isShowTransportView = value;
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
            IsShowTransportsView = false;
            IsShowStopsView = true;
            mainSettings = settingsModelView;
			stopModelView = new StopModelView(newContext, settingsModelView, UseGps);
			routsModelview = new RoutsModelView(newContext);
		}

	    private RelayCommand toggleFindViewCommanBack;

	    public RelayCommand TogleToShowStopsFindViewCommand
	    {
	        get
	        {
	            if (toggleFindViewCommanBack == null)
	                toggleFindViewCommanBack = new RelayCommand(() =>
	                {
	                    IsShowStopsView = true;
	                    IsShowTransportsView = false;
                        TogleToShowStopsFindViewCommand.RaiseCanExecuteChanged();
                        TogleToShowRoutsFindViewCommand.RaiseCanExecuteChanged();
                    }, () => !IsShowStopsView);
	            return toggleFindViewCommanBack;
	        }
	    }

        private RelayCommand toggleRoutsViewCommanBack;

        public RelayCommand TogleToShowRoutsFindViewCommand
        {
            get
            {
                if (toggleRoutsViewCommanBack== null)
                    toggleRoutsViewCommanBack = new RelayCommand(() =>
                    {
                        IsShowStopsView = false;
                        IsShowTransportsView = true;
                        TogleToShowRoutsFindViewCommand.RaiseCanExecuteChanged();
                        TogleToShowStopsFindViewCommand.RaiseCanExecuteChanged();
                    }, () => !IsShowTransportsView);
                return toggleRoutsViewCommanBack;
            }
        }
    }
}
