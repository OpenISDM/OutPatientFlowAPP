/*
 * 2020 © Copyright (c) BiDaE Technology Inc. 
 * Provided under BiDaE SHAREWARE LICENSE-1.0 in the LICENSE.
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
using System.Xml;
using IndoorNavigation.Utilities;

namespace IndoorNavigation.Models
{
    public class ClinicPositionInfo
    {
        private Dictionary<string, RegionWaypointPoint> _clinicsPositions;

        public ClinicPositionInfo()
        {

            Guid Dguid, Rguid;
            _clinicsPositions = new Dictionary<string, RegionWaypointPoint>();

            XmlDocument doc =
                Storage.XmlReader("Yuanlin_OPFM.CareRoomMapp.xml");

            XmlNodeList destinationList =
                doc.SelectNodes("navigation_graph/regions/region/Destination");

            foreach (XmlNode destinationNode in destinationList)
            {
                Dguid =
                    new Guid(destinationNode.Attributes["id"].Value);
                Rguid =
                    new Guid(destinationNode.ParentNode.Attributes["id"]
                    .Value);

                _clinicsPositions
                .Add(destinationNode.Attributes["name"].Value,
                     new RegionWaypointPoint(Rguid, Dguid));
            }
        }
        public Guid GetRegionID(string key)
        {
            try
            {
                return _clinicsPositions[key]._regionID;
            }
            catch (Exception exc)
            {
                Console.WriteLine("DictionaryError : " + exc.Message);
                return new Guid();
            }
        }
        public Guid GetWaypointID(string key)
        {
            try
            {
                return _clinicsPositions[key]._waypointID;
            }
            catch (Exception exc)
            {
                Console.WriteLine("Try to get waypointID dictionary Error :"
                    + exc.Message);
                return new Guid();
            }
        }
        public RegionWaypointPoint GetWaypoint(string key)
        {
            try
            {
                return _clinicsPositions[key];
            }
            catch (Exception exc)
            {
                Console.WriteLine("Get waypoint error" + exc.Message);

                return new RegionWaypointPoint(new Guid(), new Guid());
            }
        }
    }
}