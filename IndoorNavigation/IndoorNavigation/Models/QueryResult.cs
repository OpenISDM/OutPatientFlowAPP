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
 *      QueryResult.cs
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
using System.Collections.ObjectModel;
using Plugin.Multilingual;
using System.Resources;
using IndoorNavigation.Resources.Helpers;
using System.Reflection;
using IndoorNavigation.Views.Navigation;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IndoorNavigation
{
    public class RgRecord : DestinationItem
    {
        public string OpdDate { get; set; }
        public string DptName { get; set; }
        public string Shift { get; set; }
        public string CareRoom { get; set; }
        public string DrName { get; set; }
        public string SeeSeq { get; set; }
        public bool isComplete { get; set; }
        public bool isAccept { get; set; }
        public override string ToString() => _waypointName;
       
    }
    public class DestinationItem
    {
        public Guid _regionID { get; set; }
        public Guid _waypointID { get; set; }
        public string _waypointName { get; set; }
        public string _floor { get; set; }
        public RecordType type { get; set; }
        public override string ToString() => _waypointName;
    }

    public enum RecordType
    {
        NULL=0,
        Register,
        Queryresult,
        AddItem,
        Pharmacy,
        Cashier,
        Exit,
        Invalid
    }
}
