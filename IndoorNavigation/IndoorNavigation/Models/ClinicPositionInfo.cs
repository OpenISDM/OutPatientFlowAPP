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
 *      ClinicPositionInfo.cs
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
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Xamarin.Forms;
using IndoorNavigation.Modules.Utilities;
using IndoorNavigation.Models.NavigaionLayer;
using IndoorNavigation.Models;
namespace IndoorNavigation.Models
{
    class ClinicPositionInfo
    {
        private Dictionary<String, RegionWaypointPoint> _clinicsPositions;

        public ClinicPositionInfo()
        {
            
            Guid Dguid,Rguid;
            _clinicsPositions = new Dictionary<string, RegionWaypointPoint>();

            XmlDocument doc = 
				NavigraphStorage.XmlReader("Yuanlin_OPFM.CareRoomMapp.xml");           

            XmlNodeList destinationList = 
				doc.SelectNodes("navigation_graph/regions/region/Destination");

            foreach (XmlNode destinationNode in destinationList)
            {
                Dguid = 
					new Guid(destinationNode.Attributes["id"].Value);
                Rguid = 
					new Guid(destinationNode.ParentNode.Attributes["id"].Value);

                _clinicsPositions
				.Add(destinationNode.Attributes["name"].Value,
					 new RegionWaypointPoint(Rguid,Dguid));

                Console.WriteLine($"ID={destinationNode.Attributes["id"].Value}," 
							+$"Name={destinationNode.Attributes["name"].Value}," 
							+$"Region id={Rguid.ToString()}");               
            }
        }
        public Guid GetRegionID(string key)
        {
            if (!_clinicsPositions.ContainsKey(key))
                return new Guid();
            return _clinicsPositions[key]._regionID;
        }
        public Guid GetWaypointID(string key)
        {
            if (!_clinicsPositions.ContainsKey(key))
                return new Guid();
            return _clinicsPositions[key]._waypointID;
        }
    }    
}
