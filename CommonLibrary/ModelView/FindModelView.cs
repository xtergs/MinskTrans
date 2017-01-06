﻿using GalaSoft.MvvmLight.Command;
using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.Geopositioning;
using MinskTrans.Context.UniversalModelView;
using MinskTrans.Context.Utilites;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Modelview;
using MyLibrary;
using UniversalMinskTransRelease.ModelView;

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

	    public StopModelView StopModelView { get; }
	    public RoutsModelView RoutsModelView { get; }

	    private bool isShowStopsView;
	    public ISettingsModelView MainSettings { get; private set; }

	    public FindModelView(StopModelView stopModelView, RoutsModelView routsModelView, IBussnessLogics newContext,
            ISettingsModelView settingsModelView, bool UseGPS) : base(newContext)
        {
            IsShowTransportsView = false;
            IsShowStopsView = true;
            MainSettings = settingsModelView;
			StopModelView = stopModelView;
            StopModelView.SetGPS();
			RoutsModelView = routsModelView;
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
