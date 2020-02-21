/*
 * Copyright (c) 2019 Academia Sinica, Institude of Information Science
 *
 * License:
 *      GPL 3.0 : The content of this file is subject to the terms and
 *      conditions defined in file 'COPYING.txt', which is part of this source
 *      code package.
 *
 * Project Name:
 *
 *      IndoorNavigation
 *
 * 
 *     
 *      
 * Version:
 *
 *      1.0.0, 20200221
 * 
 * File Name:
 *
 *      ListDataTemplates.cs
 *
 * Abstract:
 *      
 *
 *      
 * Authors:
 *
 *      Jason Chang, jasonchang@iis.sinica.edu.tw
 *     
 */


using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace IndoorNavigation
{

    class ListDataTemplates : DataTemplateSelector
    {
        public DataTemplate NotCompleteTemplate { get; set; }
        public DataTemplate CompleteTemplate { get; set; }
        public DataTemplate YetNavigationTemplate { get; set; }
        public DataTemplate NullTemplate { get; set; }
        protected override DataTemplate OnSelectTemplate
										(Object item,BindableObject container)
        {
            var o = item as RgRecord;
            if (o.isAccept || o.type.Equals(RecordType.Invalid)) 
				return CompleteTemplate;
			
            if (o.type.Equals(RecordType.Queryresult)) 
				return YetNavigationTemplate;
			
            if (o.type.Equals(RecordType.NULL)) 
				return NullTemplate;
			
            return NotCompleteTemplate;            
        }
    }
}
