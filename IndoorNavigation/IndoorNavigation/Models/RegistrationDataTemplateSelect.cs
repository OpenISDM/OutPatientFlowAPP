using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace IndoorNavigation
{
    /*the class is to let Registerlist listview cell show various template*/
    class RegistrationDataTemplateSelect : DataTemplateSelector
    {
        public DataTemplate NotCompleteTemplate { get; set; }
        public DataTemplate CompleteTemplate { get; set; }
        public DataTemplate YetNavigationTemplate { get; set; }
        //public DataTemplate NullTemplate { get; set; }
        protected override DataTemplate OnSelectTemplate
                                        (Object item, BindableObject container)
        {
            var o = item as RgRecord;
            if (o.isAccept || o.type.Equals(RecordType.Invalid))
                return CompleteTemplate;

            if (o.type.Equals(RecordType.Queryresult))
                return YetNavigationTemplate;          

            return NotCompleteTemplate;
        }
    }
}
