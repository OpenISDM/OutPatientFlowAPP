using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace IndoorNavigation
{
    class RegistrationDataTemplateSelect:DataTemplateSelector
    {
        public DataTemplate NotCompleteTemplate { get; set; }
        public DataTemplate CompleteTemplate { get; set; }
        public DataTemplate YetNavigationTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(Object item,BindableObject container)
        {
            var o = item as RgRecord;
            if (o.isAccept) return CompleteTemplate;
            if (o.Key.Equals("QueryResult")) return YetNavigationTemplate;
            if (o.Key.Equals("AddItem") || o.isComplete) return NotCompleteTemplate;
            
            return CompleteTemplate;
        }
    }
}
