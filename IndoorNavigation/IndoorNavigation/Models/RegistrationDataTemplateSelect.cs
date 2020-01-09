using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace IndoorNavigation
{
    /*the class is to let Registerlist listview cell show various template*/
    class RegistrationDataTemplateSelect:DataTemplateSelector
    {
        public DataTemplate NotCompleteTemplate { get; set; }
        public DataTemplate CompleteTemplate { get; set; }
        public DataTemplate YetNavigationTemplate { get; set; }
        public DataTemplate NullTemplate { get; set; }
        protected override DataTemplate OnSelectTemplate(Object item,BindableObject container)
        {
            var o = item as RgRecord;
            if (o.isAccept) return CompleteTemplate;
            if (o.Key.Equals("QueryResult")) return YetNavigationTemplate;
            if (o.Key.Equals("AddItem") || o.isComplete || o.Key.Equals("register") || o.Key.Equals("exit") || o.Key.Equals("Pharmacy") || o.Key.Equals("Cashier")) return NotCompleteTemplate;
            if (o.Key.Equals("NULL")) return NullTemplate;
            return CompleteTemplate;
        }
    }
}
