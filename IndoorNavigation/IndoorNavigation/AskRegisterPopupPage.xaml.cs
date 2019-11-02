using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IndoorNavigation
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AskRegisterPopupPage : PopupPage
    {
        private App app = (App)Application.Current;
        public AskRegisterPopupPage()
        {
            InitializeComponent();


        }

        async private void RegisterCancelBtn_Clicked(object sender, EventArgs e)
        {
            app.records.Add(new RgRecord
            {
                DptName = "心臟血管科",
                _waypointName = "心臟科",
                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
                _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
                Shift = "50",
                CareRoom = "0205",
                DptTime = "8:30~10:00",
                SeeSeq = "50",
                Key = "QueryResult",
                isAccept = false,
                isComplete = false
            });
            app.records.Add(new RgRecord { Key = "NULL" });
            await PopupNavigation.Instance.PopAllAsync();
        }

        async private void RegisterOKBtn_Clicked(object sender, EventArgs e)
        {
            app.records.Add(new RgRecord
            {
                DptName = "導航至掛號台",
                _regionID = new Guid("11111111-1111-1111-1111-111111111111"),
                _waypointID = new Guid("00000000-0000-0000-0000-000000000002"),
                _waypointName = "掛號台",
                Key = "register"
            });
            app.records.Add(new RgRecord { Key = "NULL" });
            await PopupNavigation.Instance.PopAllAsync();
        }
    }
}