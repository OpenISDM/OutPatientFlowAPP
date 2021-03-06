﻿using System;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using Plugin.Multilingual;
using System.Globalization;
using IndoorNavigation.Models;
using System.Reflection;
using System.Threading.Tasks;
using IndoorNavigation.Views.Navigation;
using IndoorNavigation.Models.NavigaionLayer;
using static IndoorNavigation.Utilities.Storage;

namespace IndoorNavigation.Views.PopUpPage
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AskRegisterPopupPage : PopupPage
    {
        private App app = (App)Application.Current;
        private bool ButtonLock;

        INetworkSetting networkSettings;

        string _navigationGraphName;
        XMLInformation _XmlInfo;

        public AskRegisterPopupPage(string navigraphName)
        {
            InitializeComponent();

            _navigationGraphName = navigraphName;
            _XmlInfo = LoadXmlInformation(navigraphName);

        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

            ButtonLock = false;
        }

        async private void RegisterCancelBtn_Clicked(object sender, EventArgs e)
        {
            if (ButtonLock) return;
            ButtonLock = true;

            //BusyShow(true);
            await PopupNavigation.Instance.PushAsync(new IndicatorPopupPage());
            networkSettings = DependencyService.Get<INetworkSetting>();
            bool network_ability = true;
            if (network_ability)
                await CancelorClickBack();
            else
            {
                await DisplayAlert(GetResourceString("MESSAGE_STRING"),
                      GetResourceString("BAD_NETWORK_STRING"),
                      GetResourceString("OK_STRING"),
                      GetResourceString("NO_STRING"));
                ButtonLock = false;
                return;
            }
            //BusyShow(false);
            await PopupNavigation.Instance.PopAllAsync();
        }

        async private void RegisterOKBtn_Clicked(object sender, EventArgs e)
        {
            if (ButtonLock) return;
            //ResetAllState();
            ButtonLock = true;
            app.getRigistered = true;
            int order =
                       app.OrderDistrict.ContainsKey(0) ?
                       app.OrderDistrict[0] : 1;

            RgRecord record = new RgRecord
            {
                DptName = GetResourceString("NAVIGATE_TO_REGISTER_STRING"),
                _regionID = new Guid("22222222-2222-2222-2222-222222222222"),
                _waypointID = new Guid("00000000-0000-0000-0000-000000000018"),
                _waypointName = GetResourceString("REGISTERED_COUNTER_STRING"),
                type = RecordType.Register,
                isComplete = true,
                order = order++
            };

            app._globalNavigatorPage = new NavigatorPage(_navigationGraphName,
                record._regionID,
                record._waypointID,
                record._waypointName,
                _XmlInfo);
            await Navigation.PushAsync(app._globalNavigatorPage);

            await PopupNavigation.Instance.PopAllAsync();
            app.records.Add(record);
            MessagingCenter.Send(this, "isReset", true);
            ButtonLock = true;
        }

        protected override bool OnBackgroundClicked()
        {
            return false;
        }
        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        //HttpRequest request = new HttpRequest();
        //YunalinHttpRequestFake FakeHISRequest = new YunalinHttpRequestFake();
        async private Task CancelorClickBack()
        {
            app.getRigistered = false;
            await Task.CompletedTask;
        }
        //private void ResetAllState()
        //{
        //    app.records.Clear();
        //    app._TmpRecords.Clear();
        //    app.HaveCashier = false;
        //    app.FinishCount = 0;
        //    app.roundRecord = null;
        //    app.lastFinished = null;
        //    app.OrderDistrict.Clear();
        //}
        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            OnBackButtonPressed();
        }
    }
}