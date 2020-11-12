/*
 * 2020 © Copyright (c) BiDaE Technology Inc. 
 * Provided under BiDaE SHAREWARE LICENSE-1.0 in the LICENSE.
 *
 * Project Name:
 *
 *      IndoorNavigation
 *
 * Version:
 *
 *      1.0.0, 20200221
 * 
 * File Name:
 *
 *      AddPopupPage.cs
 *
 * Abstract:
 *      
 *
 *      
 * Authors:
 * 
 *      Jason Chang, jasonchang@bidae.tech 
 *      
 */
using System;
using Xamarin.Forms;

namespace IndoorNavigation.Models
{
    /*the class is to let Registerlist listview cell show various template*/
    class RegistrationDataTemplateSelect : DataTemplateSelector
    {
        public DataTemplate NotCompleteTemplate { get; set; }
        public DataTemplate CompleteTemplate { get; set; }
        public DataTemplate YetNavigationTemplate { get; set; }
        public DataTemplate AddItemDataTemplate { get; set; }
        //public DataTemplate NullTemplate { get; set; }
        protected override DataTemplate OnSelectTemplate
            (Object item, BindableObject container)
        {
            var o = item as RgRecord;
            if (o.isAccept || o.type.Equals(RecordType.Invalid))
                return CompleteTemplate;
            if (o.type.Equals(RecordType.AddItem))
                return AddItemDataTemplate;

            if (o.type.Equals(RecordType.Queryresult))
                return YetNavigationTemplate;          

            return NotCompleteTemplate;
        }
    }
}
