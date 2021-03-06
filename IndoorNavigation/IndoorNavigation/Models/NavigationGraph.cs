﻿/*
 * 2020 © Copyright (c) BiDaE Technology Inc. 
 * Provided under BiDaE SHAREWARE LICENSE-1.0 in the LICENSE.
 *
 * Project Name:
 *
 *      IndoorNavigation
 *
 * File Description:
 * 
 *      This model contains nest class: Navigraph class and also contains
 *      classes of Waypoint, Neighbor and Edge/Connection which as 
 *      the XML element.
 *      Navigraph class in a nest class that defines the structure of
 *      navigation sub-graphs of a region, and this class also has the method
 *      to get RegionGraph.
 *      
 * Version:
 *
 *      1.0.0, 20190605
 * 
 * File Name:
 *
 *      NavigationGraph.cs
 *
 * Abstract:
 *
 *      Waypoint-based navigator is a mobile Bluetooth navigation application
 *      that runs on smart phones. It is structed to support
 *      anywhere navigation. Indoors in areas covered by different
 *      indoor positioning system (IPS) and outdoors covered by GPS.
 *      In particilar, it can rely on BeDIS (Building/environment Data and
 *      Information System) for indoor positioning. 
 *      Using this IPS, the navigator does not need to continuously monitor
 *      its own position, since the IPS broadcast to the navigator the
 *      location of each waypoint.
 *      This version makes use of Xamarin.Forms, which is a complete 
 *      cross-platform UI tookit that runs on both iOS and Android.
 *
 * Authors:
 *
 *      Paul Chang, paulchang@iis.sinica.edu.tw
 *      Eric Lee, ericlee@iis.sinica.edu.tw
 *      Chun-Yu Lai, chunyu1202@gmail.com
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Dijkstra.NET.Model;
using System.Xml;
using static IndoorNavigation.Utilities.Storage;
using static IndoorNavigation.Utilities.Helper;
namespace IndoorNavigation.Models.NavigaionLayer
{
    public class NavigationGraph
    {
        private const double EARTH_RADIUS = 6378137;

        #region Navi-Graph basic infos.
        private string _country;
        private string _cityCounty;
        private string _industryService;
        private string _ownerOrganization;
        private string _buildingName;
        private double _version;
        #endregion

        //this Guid is region's Guid
        private Dictionary<Guid, Region> _regions;
        //first guid in tuple is source region's guid, 
        //second one is destination region's guid.
        private Dictionary<Tuple<Guid, Guid>, List<RegionEdge>> _edges
        { get; set; }
        private Dictionary<string, string> _banners;
        private Dictionary<Guid, Navigraph> _navigraphs { get; set; }

        #region Nest structures and classes
        public class Navigraph
        {
            public Guid _regionID { get; set; }

            //Guid is waypoint's Guid
            public Dictionary<Guid, Waypoint> _waypoints { get; set; }

            public Dictionary<Tuple<Guid, Guid>, WaypointEdge> _edges { get; set; }

            //Guid is waypoint's Guid
            public Dictionary<Guid, List<Guid>> _beacons { get; set; }

            public Dictionary<Guid, int> _beaconRSSIThreshold { get; set; }
        }

        public struct RegionEdge
        {
            public Guid _region1 { get; set; }
            public Guid _region2 { get; set; }
            public Guid _waypoint1 { get; set; }
            public Guid _waypoint2 { get; set; }
            public DirectionalConnection _biDirection { get; set; }
            public int _source { get; set; }
            public double _distance { get; set; }
            public CardinalDirection _direction { get; set; }
            public ConnectionType _connectionType { get; set; }
            public bool _isVirtualEdge { get; set; }
            public string _picture12 { get; set; }
            public string _picture21 { get; set; }
        }

        public struct WaypointEdge
        {
            public Guid _node1 { get; set; }
            public Guid _node2 { get; set; }
            public DirectionalConnection _biDirection { get; set; }
            public int _source { get; set; }
            public CardinalDirection _direction { get; set; }
            public ConnectionType _connectionType { get; set; }
            public double _distance { get; set; }
            public string _picture12 { get; set; }
            public string _picture21 { get; set; }
            // some stair, escalator or elevator can't be seen at previous 
            // point.
            public bool _supportCombine { get; set; }
        }
        #endregion

        #region Get naviGraph attributes
        public string GetOwnerOrganization()
        {
            return _ownerOrganization;
        }
        public string GetCityCountry()
        {
            return _cityCounty;
        }
        public string GetCountry()
        {
            return _country;
        }
        public string GetIndustryServer()
        {
            return _industryService;
        }
        public string GetBuildingName()
        {
            return _buildingName;
        }

        public double GetVersion()
        {
            return _version;
        }
        #endregion
        public NavigationGraph(XmlDocument xmlDocument)
        {
            Console.WriteLine(">> NavigationGraph");
            // initialize structures
            _regions = new Dictionary<Guid, Region>();
            _edges = new Dictionary<Tuple<Guid, Guid>, List<RegionEdge>>();
            _navigraphs = new Dictionary<Guid, Navigraph>();
            _banners = new Dictionary<string, string>();
            #region Read navigation_graph attrs
            //Console.WriteLine("Read attributes of <navigation_graph>");
            XmlElement elementInNavigationGraph =
                (XmlElement)xmlDocument.SelectSingleNode("navigation_graph");
            _country =
                elementInNavigationGraph.GetAttribute("country");

            _cityCounty =
                elementInNavigationGraph.GetAttribute("city_county");

            _industryService =
                elementInNavigationGraph.GetAttribute("industry_service");

            _ownerOrganization =
                elementInNavigationGraph.GetAttribute("owner_organization");

            _buildingName =
                elementInNavigationGraph.GetAttribute("building_name");

            _version =
                Convert.ToDouble(elementInNavigationGraph
                .GetAttribute("version"));

            #endregion

            #region Read Banners
            XmlNodeList BannerNodeList =
                xmlDocument.SelectNodes("navigation_graph/Banners/Banner");

            foreach (XmlNode BannerNode in BannerNodeList)
            {
                XmlElement bannerElement = (XmlElement)BannerNode;
                _banners.Add(bannerElement.GetAttribute("language"),
                    bannerElement.GetAttribute("text"));
            }
            #endregion

            #region Read navigation_graph/regions/region
            //Console.WriteLine("Read attributes of <navigation_graph>" +
            //    "<regions>/<region>");
            XmlNodeList xmlRegion =
                xmlDocument.SelectNodes("navigation_graph/regions/region");
            foreach (XmlNode regionNode in xmlRegion)
            {
                Region region = new Region();

                // initialize structures
                region._neighbors = new List<Guid>();
                region._waypointsByCategory =
                    new Dictionary<CategoryType, List<Waypoint>>();

                // Read all attributes of each region
                XmlElement xmlElement = (XmlElement)regionNode;
                region._id = Guid.Parse(xmlElement.GetAttribute("id"));
                Console.WriteLine("id : " + region._id);

                region._IPSType =
                    (IPSType)Enum.Parse(typeof(IPSType),
                                        xmlElement.GetAttribute("ips_type"),
                                        false);
                //Console.WriteLine("ips_type : " + region._IPSType);

                region._name = xmlElement.GetAttribute("name");
                //Console.WriteLine("name : " + region._name);

                region._floor = int.Parse(xmlElement.GetAttribute("floor"));
                //Console.WriteLine("floor : " + region._floor);

                // Read all <waypoint> within <region>
                //Console.WriteLine("Read attributes of <navigation_graph><regions>/" +
                //"<region>/<waypoint>");
                XmlNodeList xmlWaypoint = regionNode.SelectNodes("waypoint");
                foreach (XmlNode waypointNode in xmlWaypoint)
                {
                    Waypoint waypoint = new Waypoint();

                    // initialize structures
                    waypoint._neighbors = new List<Guid>();

                    //Read all attributes of each waypint
                    XmlElement xmlWaypointElement =
                        (XmlElement)waypointNode;

                    waypoint._id =
                        Guid.Parse(xmlWaypointElement.GetAttribute("id"));
                    //Console.WriteLine("id : " + waypoint._id);

                    waypoint._name = xmlWaypointElement.GetAttribute("name");
                    //Console.WriteLine("name : " + waypoint._name);

                    waypoint._type =
                        (LocationType)Enum.Parse(typeof(LocationType),
                        xmlWaypointElement.GetAttribute("type"), false);

                    //Console.WriteLine("type : " + waypoint._type);

                    waypoint._lon =
                        double.Parse(xmlWaypointElement.GetAttribute("lon"));
                    //Console.WriteLine("lon : " + waypoint._lon);

                    waypoint._lat =
                        double.Parse(xmlWaypointElement.GetAttribute("lat"));
                    //Console.WriteLine("lat : " + waypoint._lat);

                    waypoint._category =
                        (CategoryType)Enum.Parse(typeof(CategoryType),
                                                 xmlWaypointElement
                                                 .GetAttribute("category"),
                                                 false);
                    if (xmlWaypointElement.HasAttribute("virutalpoint"))
                    {
                        waypoint._isVirtualPoint =
                          XmlConvert.ToBoolean
                          (xmlWaypointElement.GetAttribute("virutalpoint"));
                    }
                    if (!region._waypointsByCategory.ContainsKey
                        (waypoint._category))
                    {
                        region._waypointsByCategory.Add
                            (waypoint._category,
                            new List<Waypoint>() { waypoint });
                    }
                    else
                    {
                        region._waypointsByCategory[waypoint._category]
                            .Add(waypoint);
                    }


                }
                _regions.Add(region._id, region);
            }
            #endregion

            #region Read Edge
            // Read all <edge> block within <regions>
            //Console.WriteLine("Read attributes of <navigation_graph><regions>/<edge>");
            XmlNodeList xmlRegionEdge = xmlDocument.SelectNodes("navigation_graph/regions/edge");
            foreach (XmlNode regionEdgeNode in xmlRegionEdge)
            {
                RegionEdge regionEdge = new RegionEdge();

                // Read all attributes of each edge
                XmlElement xmlElement = (XmlElement)regionEdgeNode;
                regionEdge._region1 =
                    Guid.Parse(xmlElement.GetAttribute("region1"));
                //Console.WriteLine("region1 : " + regionEdge._region1);

                regionEdge._waypoint1 =
                    Guid.Parse(xmlElement.GetAttribute("waypoint1"));
                //Console.WriteLine("waypoint1 : " + regionEdge._waypoint1);

                regionEdge._region2 =
                    Guid.Parse(xmlElement.GetAttribute("region2"));
                //Console.WriteLine("region2 : " + regionEdge._region1);

                regionEdge._waypoint2 =
                    Guid.Parse(xmlElement.GetAttribute("waypoint2"));
                //Console.WriteLine("waypoint2 : " + regionEdge._waypoint2);

                regionEdge._biDirection =
                    (DirectionalConnection)Enum
                    .Parse(typeof(DirectionalConnection),
                    xmlElement.GetAttribute("bi_direction"),
                    false);
                //Console.WriteLine("bi_direction : " + regionEdge._biDirection);

                regionEdge._source = 0;
                if (!String.IsNullOrEmpty(xmlElement.GetAttribute("source")))
                {
                    regionEdge._source =
                        int.Parse(xmlElement.GetAttribute("source"));
                }
                //Console.WriteLine("source : " + regionEdge._source);

                regionEdge._direction =
                    (CardinalDirection)Enum.Parse(typeof(CardinalDirection),
                                                  xmlElement.GetAttribute("direction"),
                                                  false);
                //Console.WriteLine("direction : " + regionEdge._direction);

                if (xmlElement.HasAttribute("isvirtualEdge"))
                {
                    regionEdge._isVirtualEdge =
                        XmlConvert.ToBoolean
                        (xmlElement.GetAttribute("isvirtualEdge"));
                }

                regionEdge._picture12 = xmlElement.GetAttribute("picture12") ?? " ";
                regionEdge._picture21 = xmlElement.GetAttribute("picture21") ?? " ";
                
                regionEdge._connectionType =
                    (ConnectionType)Enum.Parse(typeof(ConnectionType),
                    xmlElement.GetAttribute("connection_type"),
                    false);
                //Console.WriteLine("connection_type : " +
                //regionEdge._connectionType);

                // calculate the distance of this edge
                regionEdge._distance = 0;
                int distanceElevator = 3;
                int distanceEscalator = 5;
                int distanceStair = 7;
                if (ConnectionType.Elevator == regionEdge._connectionType)
                {
                    regionEdge._distance = distanceElevator;
                }
                else if (ConnectionType.Escalator ==
                    regionEdge._connectionType)
                {
                    regionEdge._distance = distanceEscalator;
                }
                else if (ConnectionType.Stair ==
                         regionEdge._connectionType)
                {
                    regionEdge._distance = distanceStair;
                }
                else if (ConnectionType.NormalHallway ==
                         regionEdge._connectionType)
                {
                    bool foundNode1 = false;
                    double node1Lon = 0;
                    double node1Lat = 0;
                    bool foundNode2 = false;
                    double node2Lon = 0;
                    double node2Lat = 0;
                    foreach (KeyValuePair<CategoryType, List<Waypoint>>
                        categoryItem in
                        _regions[regionEdge._region1]._waypointsByCategory)
                    {

                        foreach (Waypoint waypoint in categoryItem.Value)
                        {
                            if (waypoint._id.Equals(regionEdge._waypoint1))
                            {
                                node1Lon = waypoint._lon;
                                node1Lat = waypoint._lat;
                                foundNode1 = true;
                                break;
                            }
                            if (foundNode1)
                                break;
                        }
                    }
                    foreach (KeyValuePair<CategoryType, List<Waypoint>> categoryItem in
                        _regions[regionEdge._region2]._waypointsByCategory)
                    {

                        foreach (Waypoint waypoint in categoryItem.Value)
                        {
                            if (waypoint._id.Equals(regionEdge._waypoint2))
                            {
                                node2Lon = waypoint._lon;
                                node2Lat = waypoint._lat;
                                foundNode2 = true;
                                break;
                            }
                            if (foundNode2)
                                break;
                        }
                    }

                    regionEdge._distance = GetDistance(node1Lon,
                                                       node1Lat,
                                                       node2Lon,
                                                       node2Lat);

                }
                Console.WriteLine("distance : " + regionEdge._distance);


                // fill data into _edges structure
                Tuple<Guid, Guid> edgeKey =
                    new Tuple<Guid, Guid>(regionEdge._region1, regionEdge._region2);
                if (!_edges.ContainsKey(edgeKey))
                {
                    _edges.Add(edgeKey, new List<RegionEdge>() { regionEdge });
                }
                else
                {
                    _edges[edgeKey].Add(regionEdge);
                }

                // construct navigation_graph._regions[]._neighbors
                if (DirectionalConnection.BiDirection == regionEdge._biDirection)
                {
                    _regions[regionEdge._region1]._neighbors.Add(regionEdge._region2);
                    _regions[regionEdge._region2]._neighbors.Add(regionEdge._region1);
                }
                else
                {
                    if (1 == regionEdge._source)
                    {
                        _regions[regionEdge._region1]._neighbors.Add(regionEdge._region2);
                    }
                    else if (2 == regionEdge._source)
                    {
                        _regions[regionEdge._region2]._neighbors.Add(regionEdge._region1);
                    }
                }
            }
            #endregion
            #region read navigraph block of BuildingName.xml
            // Read all <navigraph> blocks within <navigraphs>
            //Console.WriteLine("Read attributes of <navigation_graph><navigraphs>/<navigraph>");
            XmlNodeList xmlNavigraph =
                xmlDocument.SelectNodes("navigation_graph/navigraphs/navigraph");
            foreach (XmlNode navigraphNode in xmlNavigraph)
            {
                Navigraph navigraph = new Navigraph();

                // initialize structures
                navigraph._waypoints = new Dictionary<Guid, Waypoint>();
                navigraph._edges = new Dictionary<Tuple<Guid, Guid>, WaypointEdge>();
                navigraph._beacons = new Dictionary<Guid, List<Guid>>();
                navigraph._beaconRSSIThreshold = new Dictionary<Guid, int>();
                // Read all attributes of each navigraph
                XmlElement xmlElement = (XmlElement)navigraphNode;
                navigraph._regionID = Guid.Parse(xmlElement.GetAttribute("region_id"));
                //Console.WriteLine("region_id : " + navigraph._regionID);

                // Read all <waypoint> within <navigraph>
                //Console.WriteLine("Read attributes of <navigation_graph><navigraphs>/" +
                //"<navigraph>/<waypoint>");
                XmlNodeList xmlWaypoint = navigraphNode.SelectNodes("waypoint");
                foreach (XmlNode waypointNode in xmlWaypoint)
                {
                    Waypoint waypoint = new Waypoint();

                    // initialize structures
                    waypoint._neighbors = new List<Guid>();

                    //Read all attributes of each waypint
                    XmlElement xmlWaypointElement = (XmlElement)waypointNode;
                    waypoint._id =
                        Guid.Parse(xmlWaypointElement.GetAttribute("id"));
                    //Console.WriteLine("id : " + waypoint._id);

                    waypoint._name = xmlWaypointElement.GetAttribute("name");
                    //Console.WriteLine("name : " + waypoint._name);

                    waypoint._type =
                        (LocationType)Enum.Parse(typeof(LocationType),
                        xmlWaypointElement.GetAttribute("type"), false);
                    //Console.WriteLine("type : " + waypoint._type);

                    waypoint._lon =
                        double.Parse(xmlWaypointElement.GetAttribute("lon"));
                    //Console.WriteLine("lon : " + waypoint._lon);

                    waypoint._lat =
                        double.Parse(xmlWaypointElement.GetAttribute("lat"));
                    //Console.WriteLine("lat : " + waypoint._lat);

                    waypoint._category =
                        (CategoryType)Enum.Parse(typeof(CategoryType),
                        xmlWaypointElement.GetAttribute("category"), false);
                    //Console.WriteLine("category : " + waypoint._category);
                    navigraph._waypoints.Add(waypoint._id, waypoint);
                }
                #endregion
                #region read edges block
                // Read all <edge> block within <navigraph>
                //Console.WriteLine("Read attributes of <navigation_graph><navigraphs>/" +
                //"<navigraph>/<edge>");
                XmlNodeList xmlWaypointEdge =
                    navigraphNode.SelectNodes("edge");
                foreach (XmlNode waypointEdgeNode in xmlWaypointEdge)
                {
                    WaypointEdge waypointEdge = new WaypointEdge();

                    // Read all attributes of each edge
                    XmlElement xmlEdgeElement = (XmlElement)waypointEdgeNode;
                    waypointEdge._node1 =
                        Guid.Parse(xmlEdgeElement.GetAttribute("node1"));
                    //Console.WriteLine("node1 : " + waypointEdge._node1);

                    waypointEdge._node2 =
                        Guid.Parse(xmlEdgeElement.GetAttribute("node2"));
                    //Console.WriteLine("node2 : " + waypointEdge._node2);

                    waypointEdge._biDirection =
                        (DirectionalConnection)Enum.Parse
                        (typeof(DirectionalConnection),
                        xmlEdgeElement.GetAttribute("bi_direction"),
                        false);
                    //Console.WriteLine("bi_direction : " + waypointEdge._biDirection);

                    waypointEdge._source = 0;
                    if (!string.IsNullOrEmpty
                        (xmlEdgeElement.GetAttribute("source")))
                    {
                        waypointEdge._source =
                            int.Parse(xmlEdgeElement.GetAttribute("source"));
                    }
                    Console.WriteLine("source : " + waypointEdge._source);

                    waypointEdge._direction =
                        (CardinalDirection)Enum.Parse
                        (typeof(CardinalDirection),
                        xmlEdgeElement.GetAttribute("direction"),
                        false);
                    //Console.WriteLine("direction : " + waypointEdge._direction);

                    #region read direction picture part

                    waypointEdge._picture12 = xmlEdgeElement.GetAttribute("picture12") ?? " ";
                    waypointEdge._picture21 = xmlEdgeElement.GetAttribute("picture21") ?? " ";
                    
                    //Console.WriteLine("picture 12 = "+ waypointEdge._picture12);
                    #endregion

                    waypointEdge._connectionType =
                        (ConnectionType)Enum.Parse(typeof(ConnectionType),
                                                   xmlEdgeElement
                                                   .GetAttribute
                                                   ("connection_type"),
                                                   false);
                    //Console.WriteLine("connection_type : " +
                    //    waypointEdge._connectionType);

                    waypointEdge._supportCombine =
                        xmlEdgeElement.HasAttribute("combineInstruction")
                        ? false : true;

                    //Console.WriteLine("support combine : " +
                    //    waypointEdge._supportCombine);

                    waypointEdge._distance =
                        GetDistance(navigraph._waypoints[waypointEdge._node1]._lon,
                                    navigraph._waypoints[waypointEdge._node1]._lat,
                                    navigraph._waypoints[waypointEdge._node2]._lon,
                                    navigraph._waypoints[waypointEdge._node2]._lat);
                    //Console.WriteLine("distance : " + waypointEdge._distance);

                    Tuple<Guid, Guid> edgeKey =
                        new Tuple<Guid, Guid>(waypointEdge._node1, waypointEdge._node2);
                    navigraph._edges.Add(edgeKey, waypointEdge);

                    if (DirectionalConnection.BiDirection == waypointEdge._biDirection)
                    {
                        navigraph._waypoints[waypointEdge._node1]._neighbors.Add(waypointEdge._node2);
                        navigraph._waypoints[waypointEdge._node2]._neighbors.Add(waypointEdge._node1);
                    }
                    else
                    {
                        if (1 == waypointEdge._source)
                        {
                            navigraph._waypoints[waypointEdge._node1]._neighbors
                                .Add(waypointEdge._node2);
                        }
                        else if (2 == waypointEdge._source)
                        {
                            navigraph._waypoints[waypointEdge._node2]._neighbors
                                .Add(waypointEdge._node1);
                        }
                    }
                }
                #endregion
                //Console.WriteLine("Read attributes of <navigation_graph><navigraphs>/" +
                //                  "<navigraph>/<beacons>/<beacon>");
                #region read beacons
                XmlNodeList xmlBeacon = navigraphNode.SelectNodes("beacons/beacon");
                foreach (XmlNode beaconNode in xmlBeacon)
                {
                    XmlElement xmlBeaconElement = (XmlElement)beaconNode;
                    Guid beaconGuid = Guid.Parse(xmlBeaconElement.GetAttribute("uuid"));

                    string waypointIDs = xmlBeaconElement.GetAttribute("waypoint_ids");
                    //Console.WriteLine("waypoint_ids : " + waypointIDs);

                    string[] arrayWaypointIDs = waypointIDs.Split(';');
                    for (int i = 0; i < arrayWaypointIDs.Count(); i++)
                    {
                        Guid waypointID = Guid.Parse(arrayWaypointIDs[i]);
                        if (!navigraph._beacons.ContainsKey(waypointID))
                        {
                            navigraph._beacons.Add
                                (waypointID,
                                new List<Guid>() { beaconGuid });
                        }
                        else
                        {
                            navigraph._beacons[waypointID].Add(beaconGuid);
                        }
                    }

                    int beaconRSSI =
                        int.Parse(xmlBeaconElement.GetAttribute("threshold"));
                    navigraph._beaconRSSIThreshold.Add(beaconGuid, beaconRSSI);
                }

                _navigraphs.Add(navigraph._regionID, navigraph);
            }
            #endregion
            Console.WriteLine("<< NavigationGraph");
        }

        private double GetDistance(double lon1, double lat1, double lon2,
            double lat2)
        {
            double radLat1 = Rad(lat1);
            double radLng1 = Rad(lon1);
            double radLat2 = Rad(lat2);
            double radLng2 = Rad(lon2);
            double a = radLat1 - radLat2;
            double b = radLng1 - radLng2;
            double result =
                2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
                Math.Cos(radLat1) * Math.Cos(radLat2) *
                Math.Pow(Math.Sin(b / 2), 2))) * EARTH_RADIUS;
            return result;
        }

        private double Rad(double d)
        {
            return (double)d * Math.PI / 180d;
        }

        public bool ContainsBanner()
        {
            return !(_banners.Count == 0);
        }
        public string GetBanner()
        {
            if (!_banners.ContainsKey(_currentCulture.Name))
            {
                return _banners.First().Value;
            }
            return _banners[_currentCulture.Name];
        }
        private RegionEdge GetRegionEdgeMostNearSourceWaypoint
            (Guid sourceRegionID,
            Guid sourceWaypointID,
            Guid sinkRegionID,
            ConnectionType[] avoidConnectionTypes,
            ref string directionPicture)
        {
            RegionEdge regionEdgeItem = new RegionEdge();

            Waypoint sourceWaypoint =
                _navigraphs[sourceRegionID]._waypoints[sourceWaypointID];

            // compare the normal case (R1, R2)
            Tuple<Guid, Guid> edgeKeyFromNode1 =
                new Tuple<Guid, Guid>(sourceRegionID, sinkRegionID);

            int distance = int.MaxValue;
            int indexEdge = -1;
            if (_edges.ContainsKey(edgeKeyFromNode1))
            {
                for (int i = 0; i < _edges[edgeKeyFromNode1].Count(); i++)
                {
                    RegionEdge edgeItem = _edges[edgeKeyFromNode1][i];

                    if (!avoidConnectionTypes.Contains(edgeItem._connectionType))
                    {
                        if (DirectionalConnection.BiDirection ==
                            edgeItem._biDirection ||
                        (DirectionalConnection.OneWay == edgeItem._biDirection
                        && 1 == edgeItem._source))
                        {
                            Waypoint sinkWaypoint =
                                _navigraphs[sourceRegionID]
                                ._waypoints[edgeItem._waypoint1];
                            double distanceFromSource =
                                GetDistance(sourceWaypoint._lon,
                                            sourceWaypoint._lat,
                                            sinkWaypoint._lon,
                                            sinkWaypoint._lat);
                            int edgeDistance =
                                Convert.ToInt32(distanceFromSource);

                            if (edgeDistance < distance)
                            {
                                distance = edgeDistance;
                                indexEdge = i;
                            }
                        }
                    }
                }
            }
            if (-1 != indexEdge)
            {
                regionEdgeItem = _edges[edgeKeyFromNode1][indexEdge];
                //Console.WriteLine("direction picture in NavigationGraph"
                //    + regionEdgeItem._picture12) ;
                directionPicture = regionEdgeItem._picture12;
                return regionEdgeItem;
            }

            // compare the reverse case (R2, R1) because normal case (R1, R2) 
            //cannot find regionEdge
            Tuple<Guid, Guid> edgeKeyFromNode2 =
                new Tuple<Guid, Guid>(sinkRegionID, sourceRegionID);

            if (_edges.ContainsKey(edgeKeyFromNode2))
            {

                for (int i = 0; i < _edges[edgeKeyFromNode2].Count(); i++)
                {

                    RegionEdge edgeItem = _edges[edgeKeyFromNode2][i];

                    if (!avoidConnectionTypes.Contains
                        (edgeItem._connectionType))
                    {

                        if (DirectionalConnection.BiDirection ==
                            edgeItem._biDirection ||
                            (DirectionalConnection.OneWay ==
                            edgeItem._biDirection && 2 == edgeItem._source))
                        {



                            Waypoint sinkWaypoint =
                                _navigraphs[sourceRegionID]
                                ._waypoints[edgeItem._waypoint2];

                            double distanceFromSource =
                                GetDistance(sourceWaypoint._lon,
                                            sourceWaypoint._lat,
                                            sinkWaypoint._lon,
                                            sinkWaypoint._lat);

                            int edgeDistance =
                                Convert.ToInt32(distanceFromSource);

                            if (edgeDistance < distance)
                            {
                                distance = edgeDistance;
                                indexEdge = i;
                            }
                        }
                    }
                }
            }
            if (-1 != indexEdge)
            {

                // need to reverse the resulted regionEdge from (R1/W1, R2/W2)
                // pair to (R2/W2, R1/W1) pair before returning to caller
                regionEdgeItem._region1 =
                    _edges[edgeKeyFromNode2][indexEdge]._region2;

                regionEdgeItem._region2 =
                    _edges[edgeKeyFromNode2][indexEdge]._region1;

                regionEdgeItem._waypoint1 =
                    _edges[edgeKeyFromNode2][indexEdge]._waypoint2;

                regionEdgeItem._waypoint2 =
                    _edges[edgeKeyFromNode2][indexEdge]._waypoint1;

                regionEdgeItem._biDirection =
                    _edges[edgeKeyFromNode2][indexEdge]._biDirection;

                if (2 == _edges[edgeKeyFromNode2][indexEdge]._source)
                    regionEdgeItem._source = 1;
                regionEdgeItem._distance =
                    _edges[edgeKeyFromNode2][indexEdge]._distance;
                if (Convert.ToInt32(_edges[edgeKeyFromNode2][indexEdge]
                    ._direction) + 4 < 8)
                {
                    regionEdgeItem._direction =
                        (4 + _edges[edgeKeyFromNode2][indexEdge]._direction);
                }
                else
                {
                    regionEdgeItem._direction =
                        (4 + _edges[edgeKeyFromNode2][indexEdge]._direction
                        - 8);
                }
                regionEdgeItem._connectionType =
                    _edges[edgeKeyFromNode2][indexEdge]._connectionType;
                directionPicture = regionEdgeItem._picture21;
                return regionEdgeItem;

            }

            return regionEdgeItem;
        }

        private RegionEdge GetRegionEdgeMostNearSourceWaypoint
            (Guid sourceRegionID,
             Guid sourceWaypointID,
             Guid sinkRegionID,
             ConnectionType[] avoidConnectionTypes)
        {
            RegionEdge regionEdgeItem = new RegionEdge();

            Waypoint sourceWaypoint =
                _navigraphs[sourceRegionID]._waypoints[sourceWaypointID];

            // compare the normal case (R1, R2)
            Tuple<Guid, Guid> edgeKeyFromNode1 =
                new Tuple<Guid, Guid>(sourceRegionID, sinkRegionID);

            int distance = int.MaxValue;
            int indexEdge = -1;
            if (_edges.ContainsKey(edgeKeyFromNode1))
            {
                for (int i = 0; i < _edges[edgeKeyFromNode1].Count(); i++)
                {
                    RegionEdge edgeItem = _edges[edgeKeyFromNode1][i];

                    if (!avoidConnectionTypes.Contains
                        (edgeItem._connectionType))
                    {
                        if (DirectionalConnection.BiDirection ==
                            edgeItem._biDirection ||
                        (DirectionalConnection.OneWay == edgeItem._biDirection
                        && 1 == edgeItem._source))
                        {
                            Waypoint sinkWaypoint =
                                _navigraphs[sourceRegionID]
                                ._waypoints[edgeItem._waypoint1];

                            double distanceFromSource =
                                GetDistance(sourceWaypoint._lon,
                                            sourceWaypoint._lat,
                                            sinkWaypoint._lon,
                                            sinkWaypoint._lat);
                            int edgeDistance =
                                Convert.ToInt32(distanceFromSource);

                            if (edgeDistance < distance)
                            {
                                distance = edgeDistance;
                                indexEdge = i;
                            }
                        }
                    }
                }
            }
            if (-1 != indexEdge)
            {
                regionEdgeItem = _edges[edgeKeyFromNode1][indexEdge];
                return regionEdgeItem;
            }

            // compare the reverse case (R2, R1) because normal case (R1, R2)
            // cannot find regionEdge
            Tuple<Guid, Guid> edgeKeyFromNode2 =
                new Tuple<Guid, Guid>(sinkRegionID, sourceRegionID);

            if (_edges.ContainsKey(edgeKeyFromNode2))
            {
                for (int i = 0; i < _edges[edgeKeyFromNode2].Count(); i++)
                {

                    RegionEdge edgeItem = _edges[edgeKeyFromNode2][i];

                    if (!avoidConnectionTypes.Contains
                        (edgeItem._connectionType))
                    {

                        if (DirectionalConnection.BiDirection ==
                            edgeItem._biDirection ||
                            (DirectionalConnection.OneWay ==
                            edgeItem._biDirection && 2 == edgeItem._source))
                        {
                            Waypoint sinkWaypoint =
                                _navigraphs[sourceRegionID]
                                ._waypoints[edgeItem._waypoint2];

                            double distanceFromSource =
                                GetDistance(sourceWaypoint._lon,
                                            sourceWaypoint._lat,
                                            sinkWaypoint._lon,
                                            sinkWaypoint._lat);

                            int edgeDistance =
                                Convert.ToInt32(distanceFromSource);

                            if (edgeDistance < distance)
                            {
                                distance = edgeDistance;
                                indexEdge = i;
                            }
                        }
                    }
                }
            }
            if (-1 != indexEdge)
            {

                // need to reverse the resulted regionEdge from (R1/W1, R2/W2) 
                // pair to (R2/W2, R1/W1) pair before returning to caller
                regionEdgeItem._region1 =
                    _edges[edgeKeyFromNode2][indexEdge]._region2;

                regionEdgeItem._region2 =
                    _edges[edgeKeyFromNode2][indexEdge]._region1;

                regionEdgeItem._waypoint1 =
                    _edges[edgeKeyFromNode2][indexEdge]._waypoint2;

                regionEdgeItem._waypoint2 =
                    _edges[edgeKeyFromNode2][indexEdge]._waypoint1;

                regionEdgeItem._biDirection =
                    _edges[edgeKeyFromNode2][indexEdge]._biDirection;

                if (2 == _edges[edgeKeyFromNode2][indexEdge]._source)
                    regionEdgeItem._source = 1;
                regionEdgeItem._distance =
                    _edges[edgeKeyFromNode2][indexEdge]._distance;

                if (Convert.ToInt32(_edges[edgeKeyFromNode2][indexEdge]
                    ._direction) + 4 < 8)
                {
                    regionEdgeItem._direction =
                        (4 + _edges[edgeKeyFromNode2][indexEdge]._direction);
                }
                else
                {
                    regionEdgeItem._direction =
                        (4 + _edges[edgeKeyFromNode2][indexEdge]._direction
                        - 8);
                }
                regionEdgeItem._connectionType =
                    _edges[edgeKeyFromNode2][indexEdge]._connectionType;

                return regionEdgeItem;

            }
            return regionEdgeItem;
        }

        private Tuple<WaypointEdge, string> GetWaypointEdgeInRegion
            (Guid regionID,
             Guid sourceWaypoindID,
             Guid sinkWaypointID)
        {
            WaypointEdge waypointEdge = new WaypointEdge();
            string pictureDirection = "";

            Tuple<Guid, Guid> edgeKeyFromNode1 =
                new Tuple<Guid, Guid>(sourceWaypoindID, sinkWaypointID);

            Tuple<Guid, Guid> edgeKeyFromNode2 =
                new Tuple<Guid, Guid>(sinkWaypointID, sourceWaypoindID);

            if (_navigraphs[regionID]._edges.ContainsKey(edgeKeyFromNode1))
            {
                // XML file contains (W1, W2) and the query input is 
                // (W1, W2) as well.
                waypointEdge = _navigraphs[regionID]._edges[edgeKeyFromNode1];
                pictureDirection =
                    _navigraphs[regionID]._edges[edgeKeyFromNode1]._picture12;
            }
            else if (_navigraphs[regionID]._edges.ContainsKey
                (edgeKeyFromNode2))
            {
                // XML file contains (W1, W2) but the query string is (W2, W1).
                waypointEdge = _navigraphs[regionID]._edges[edgeKeyFromNode2];
                pictureDirection =
                    _navigraphs[regionID]._edges[edgeKeyFromNode2]._picture21;
                if (Convert.ToInt32(waypointEdge._direction) + 4 < 8)
                {
                    waypointEdge._direction =
                        (4 + waypointEdge._direction);
                }
                else
                {
                    waypointEdge._direction =
                        (4 + waypointEdge._direction - 8);
                }

            }
            return new Tuple<WaypointEdge, string>
                (waypointEdge, pictureDirection);
        }


        public int GetDistanceOfLongHallway(int nextStep,
            List<RegionWaypointPoint> allRoute,
            ConnectionType[] avoidConnectionType)
        {
            int distance = 0;
            if (nextStep <= 0)
            {
                nextStep = 1;
            }
            for (int i = nextStep - 1; i < allRoute.Count(); i++)
            {
                if (!isSameGuid(allRoute[i]._regionID,
                    allRoute[i + 1]._regionID))
                {
                    if (isSameFloor(allRoute[i]._regionID,
                        allRoute[i + 1]._regionID))
                    {
                        RegionEdge regionEdge =
                            GetRegionEdgeMostNearSourceWaypoint
                            (allRoute[i]._regionID,
                             allRoute[i]._waypointID,
                             allRoute[i + 1]._regionID,
                             avoidConnectionType);

                        distance = Convert.ToInt32(regionEdge._distance);
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    WaypointEdge waypointEdge =
                        GetWaypointEdgeInRegion(allRoute[i]._regionID,
                        allRoute[i]._waypointID, allRoute[i + 1]._waypointID)
                        .Item1;

                    distance += Convert.ToInt32(waypointEdge._distance);

                    if (i + 2 >= allRoute.Count())
                    {
                        break;
                    }
                    else
                    {
                        WaypointEdge currentWaypointEdge =
                            GetWaypointEdgeInRegion(allRoute[i]._regionID,
                            allRoute[i]._waypointID,
                            allRoute[i + 1]._waypointID).Item1;

                        WaypointEdge nextWaypointEdge =
                            GetWaypointEdgeInRegion(allRoute[i + 1]._regionID,
                            allRoute[i + 1]._waypointID,
                            allRoute[i + 2]._waypointID).Item1;

                        if (currentWaypointEdge._direction !=
                            nextWaypointEdge._direction)
                        {
                            break;
                        }
                    }
                }
            }
            return distance;
        }

        public Dictionary<Guid, Region> GetRegions()
        {
            return _regions;
        }

        public List<Guid> GetAllBeaconIDInOneWaypointOfRegion(Guid regionID,
                                                              Guid waypointID)
        {
            return _navigraphs[regionID]._beacons[waypointID];
        }

        public List<Guid> GetAllWaypointIDInOneRegion(Guid regionID)
        {
            List<Guid> waypointIDs = new List<Guid>();

            foreach (KeyValuePair<Guid, Waypoint> waypointItem
                     in _navigraphs[regionID]._waypoints)
            {
                waypointIDs.Add(waypointItem.Key);
            }
            return waypointIDs;
        }

        public List<Guid> GetAllRegionIDs()
        {
            List<Guid> regionIDs = new List<Guid>();
            foreach (KeyValuePair<Guid, Region> regionItems in _regions)
            {
                regionIDs.Add(regionItems.Key);
            }

            return regionIDs;
        }

        public LocationType GetWaypointTypeInRegion(Guid regionID,
            Guid waypointID)
        {
            return _navigraphs[regionID]._waypoints[waypointID]._type;
        }

        public string GetWaypointNameInRegion(Guid regionID, Guid waypointID)
        {
            return _navigraphs[regionID]._waypoints[waypointID]._name;
        }

        public IPSType GetRegionIPSType(Guid regionID)
        {
            return _regions[regionID]._IPSType;
        }

        public int GetBeaconRSSIThreshold(Guid regionGuid, Guid beaconGuid)
        {
            return _navigraphs[regionGuid]._beaconRSSIThreshold[beaconGuid];
        }

        public PortalWaypoints GetPortalWaypoints(Guid sourceRegionID,
            Guid sourceWaypointID,
            Guid sinkRegionID,
            ConnectionType[] avoidConnectionTypes)
        {
            // for regionEdge, we need to handle following two cases:
            // case 1. R1/W1 -> R2/W2
            // case 2. R2/W2 -> R1/W1
            // When we parse the XML file, we may store either one of these two
            // cases into C# structure with its bi-direction, connectiontype, 
            // and source properties.
            // While this edge is queried, we should serve both (R1, R2) and 
            // (R2, R1) cases with corresponding portal waypoints.
            RegionEdge regionEdge =
                GetRegionEdgeMostNearSourceWaypoint(sourceRegionID,
                sourceWaypointID,
                sinkRegionID,
                avoidConnectionTypes);
            return new PortalWaypoints(regionEdge._waypoint1,
                regionEdge._waypoint2);
        }

        public RegionWaypointPoint GiveNeighborWaypointInNeighborRegion
            (Guid sourceRegionID, Guid sourceWaypointID, Guid sinkRegionID)
        {
            RegionEdge regionEdge =
                GetRegionEdgeMostNearSourceWaypoint(sourceRegionID,
                    sourceWaypointID,
                    sinkRegionID,
                    new ConnectionType[] { });

            return new RegionWaypointPoint
                (regionEdge._region2, regionEdge._waypoint2);
        }

        public List<Guid> GetNeighbor(Guid regionID, Guid waypointID)
        {
            return _navigraphs[regionID]._waypoints[waypointID]._neighbors;
        }

        public double StraightDistanceBetweenWaypoints(Guid region,
            Guid waypointID1, Guid waypointID2)
        {
            double lat1 = _navigraphs[region]._waypoints[waypointID1]._lat;
            double lon1 = _navigraphs[region]._waypoints[waypointID1]._lon;
            double lat2 = _navigraphs[region]._waypoints[waypointID2]._lat;
            double lon2 = _navigraphs[region]._waypoints[waypointID2]._lon;
            return GetDistance(lon1, lat1, lon2, lat2);
        }

        private bool isSameFloor(Guid region1, Guid region2)
        {
            try
            {
                return _regions[region1]._floor.Equals
                    (_regions[region2]._floor);
            }
            catch
            {
                return false;
            }
        }

        private TurnDirection GetTurnUpDownDirection(Guid region1,
            Guid region2)
        {
            if (_regions[region1]._floor < _regions[region2]._floor)
                return TurnDirection.Up;

            return TurnDirection.Down;
        }
        private InstructionInformation GetCombineInstruction(
            Guid currentRegionID,
            Guid currentWaypointID,
            Guid nextRegionID,
            Guid nextWaypointID,
            Guid nextnextRegionID,
            Guid nextnextWaypointID,
            ConnectionType[] avoidConnectionTypes,
            bool flag)
        {
            InstructionInformation instruction = new InstructionInformation();
            // to show instruction "please take elevator to 2F"
            if (flag)
            {
                instruction._turnDirection =
                    GetTurnUpDownDirection(nextRegionID, nextnextRegionID);

                RegionEdge currentEdge =
                    GetRegionEdgeMostNearSourceWaypoint(nextRegionID,
                    nextWaypointID,
                    nextnextRegionID,
                    avoidConnectionTypes);
                instruction._connectionType = currentEdge._connectionType;
                instruction._distance = Convert.ToInt32(currentEdge._distance);
                instruction._regionName = _regions[nextnextRegionID]._name;
                instruction._nextDirection = TurnDirection.Null;
                return instruction;
            }
            //to show instruction "please take elevator to 2F then turn right"
            else
            {
                if (isEmptyGuid(nextnextWaypointID) &&
                    isEmptyGuid(nextnextRegionID))
                {
                    //Console.WriteLine("nextnext is null");
                    RegionEdge currentEdge =
                        GetRegionEdgeMostNearSourceWaypoint(currentRegionID,
                        currentWaypointID,
                        nextRegionID,
                        avoidConnectionTypes);

                    instruction._regionName = _regions[nextRegionID]._name;

                    instruction._turnDirection =
                        GetTurnUpDownDirection(currentRegionID, nextRegionID);

                    instruction._connectionType =
                        currentEdge._connectionType;
                    instruction._nextDirection = TurnDirection.Null;
                    instruction._isVirtualWay = currentEdge._isVirtualEdge;
                    return instruction;
                }
                else
                {
                    instruction._regionName =
                       _regions[nextRegionID]._name;
                    #region To get elevator edge instructions first
                    instruction._turnDirection =
                        GetTurnUpDownDirection(currentRegionID, nextRegionID);

                    RegionEdge currentRegionEdge =
                        GetRegionEdgeMostNearSourceWaypoint(currentRegionID,
                        currentWaypointID,
                        nextRegionID,
                        avoidConnectionTypes);

                    instruction._connectionType =
                        currentRegionEdge._connectionType;

                    instruction._distance =
                        Convert.ToInt32(currentRegionEdge._distance);
                    #endregion

                    #region Get the instruction after leaving the elevator.               
                    instruction._nextDirection = TurnDirection.FirstDirection;
                    WaypointEdge nextWaypointEdge =
                        GetWaypointEdgeInRegion(nextRegionID,
                        nextWaypointID,
                        nextnextWaypointID).Item1;

                    if (nextWaypointEdge.Equals(new WaypointEdge()) ||
                        nextnextRegionID != nextRegionID)
                    {
                        //Console.WriteLine("nextWaypointEdge is null");

                        RegionEdge regionEdge =
                            GetRegionEdgeMostNearSourceWaypoint
                            (nextRegionID,
                            nextWaypointID,
                            nextnextRegionID,
                            avoidConnectionTypes);

                        instruction._relatedDirectionOfFirstDirection =
                            regionEdge._direction;
                        instruction._distance =
                            Convert.ToInt32(regionEdge._distance);
                    }
                    else
                    {
                        instruction._relatedDirectionOfFirstDirection =
                            nextWaypointEdge._direction;

                        instruction._distance =
                            Convert.ToInt32(nextWaypointEdge._distance);
                    }
                    #endregion
                    return instruction;
                }
            }
        }
        public InstructionInformation GetInstructionInformation(
            int currentNavigationStep,
            Guid previousRegionID, Guid previousWaypointID,
            Guid currentRegionID, Guid currentWaypointID,
            Guid nextRegionID, Guid nextWaypointID,
            Guid nextnextRegionID, Guid nextnextWaypointID,
            ConnectionType[] avoidConnectionTypes)
        {
            InstructionInformation information = new InstructionInformation();

            information._floor = _regions[nextRegionID]._floor;
            information._regionName = _regions[nextRegionID]._name;

            if (!(isEmptyGuid(nextnextRegionID) &&
                isEmptyGuid(nextnextWaypointID)) &&
                !isSameGuid(nextnextRegionID, nextRegionID) &&
                !isSameFloor(nextnextRegionID, nextRegionID))
            {
                WaypointEdge CurrentNextEdge =
                    GetWaypointEdgeInRegion
                    (currentRegionID, currentWaypointID,
                    nextWaypointID).Item1;

                if (CurrentNextEdge._supportCombine)
                {
                    return GetCombineInstruction(
                        currentRegionID,
                        currentWaypointID,
                        nextRegionID,
                        nextWaypointID,
                        nextnextRegionID,
                        nextnextWaypointID,
                        avoidConnectionTypes,
                        true
                        );
                }
                else
                {
                    return GetInstructionInformation(currentNavigationStep,
                        previousRegionID, previousWaypointID,
                        currentRegionID, currentWaypointID,
                        nextRegionID, nextWaypointID,
                        new Guid(), new Guid(),
                        avoidConnectionTypes);
                }
            }
            else if (!currentRegionID.Equals(nextRegionID))
            {
                // currentWaypoint and nextWaypoint are in different regions
                if (!isSameFloor(currentRegionID, nextRegionID))
                {
                    return GetCombineInstruction(
                        currentRegionID,
                        currentWaypointID,
                        nextRegionID,
                        nextWaypointID,
                        nextnextRegionID,
                        nextnextWaypointID,
                        avoidConnectionTypes,
                        false
                        );
                }
                else
                {
                    // currentWaypoint and nextWaypoint are across regions
                    // but on the same floor 

                    // When step==0, it means that the turn direction is first 
                    // direction.
                    if (0 == currentNavigationStep)
                    {
                        // currentWaypoint is the first waypoing from the 
                        // beginning need to refine the turndirection in this 
                        // case
                        information._turnDirection =
                            TurnDirection.FirstDirection;

                        string directionPicture = "";

                        RegionEdge currentEdge =
                            GetRegionEdgeMostNearSourceWaypoint
                            (currentRegionID,
                             currentWaypointID,
                             nextRegionID,
                             avoidConnectionTypes,
                             ref directionPicture);

                        SetInformationByRegionEdge(ref information,
                            currentEdge,
                            directionPicture);
                    }
                    else
                    {
                        if (!previousRegionID.Equals(currentRegionID))
                        {
                            // previouWaypoint and currentWaypoint are across 
                            // regions
                            if (!_regions[previousRegionID]._floor.Equals(
                                _regions[currentRegionID]._floor))
                            {
                                if (!currentRegionID.Equals(nextRegionID))
                                {
                                    information._turnDirection =
                                        TurnDirection.FirstDirection;

                                    string directionPicture = "";
                                    RegionEdge regionEdge =
                                        GetRegionEdgeMostNearSourceWaypoint
                                        (currentRegionID,
                                         currentWaypointID,
                                         nextRegionID,
                                         avoidConnectionTypes,
                                         ref directionPicture);

                                    SetInformationByRegionEdge(ref information,
                                        regionEdge,
                                        directionPicture);
                                }
                                else
                                {
                                    information._turnDirection =
                                        TurnDirection.FirstDirection;

                                    Tuple<WaypointEdge, string> WaypointEdgeTuple =
                                        GetWaypointEdgeInRegion
                                        (currentRegionID,
                                        currentWaypointID,
                                        nextWaypointID);

                                    SetInformationByWaypointEdge
                                        (ref information,
                                        WaypointEdgeTuple.Item1,
                                        WaypointEdgeTuple.Item2);
                                }
                                // previousWaypoint and currentWaypoint are on 
                                // different floor need to refine the turn
                                //direction in this case
                            }
                            else
                            {
                                // previousWaypoint and currentWaypoint are on
                                //the same floor
                                RegionEdge prevEdge =
                                    GetRegionEdgeMostNearSourceWaypoint
                                    (previousRegionID,
                                    previousWaypointID,
                                    currentRegionID,
                                    avoidConnectionTypes);

                                string directionPicture = "";

                                RegionEdge currentEdge =
                                    GetRegionEdgeMostNearSourceWaypoint
                                    (currentRegionID,
                                     currentWaypointID,
                                     nextRegionID,
                                     avoidConnectionTypes,
                                     ref directionPicture);

                                information._turnDirection =
                                    GetTurnDirection(currentEdge._direction, prevEdge._direction);
                                information._connectionType =
                                    currentEdge._connectionType;

                                information._distance =
                                    Convert.ToInt32(currentEdge._distance);

                                information._directionPicture =
                                    directionPicture;
                            }
                        }
                        else
                        {
                            // previousWaypoint and currentWaypoint are in the 
                            // same region
                            WaypointEdge prevEdge =
                                GetWaypointEdgeInRegion(previousRegionID,
                                                   previousWaypointID,
                                                   currentWaypointID).Item1;

                            RegionEdge currentEdge =
                                GetRegionEdgeMostNearSourceWaypoint
                                (currentRegionID,
                                 currentWaypointID,
                                 nextRegionID,
                                 avoidConnectionTypes);

                            information._turnDirection =
                                GetTurnDirection(currentEdge._direction, prevEdge._direction);

                            information._connectionType =
                                currentEdge._connectionType;
                            information._distance =
                                Convert.ToInt32(currentEdge._distance);
                        }
                    }
                }
            }
            else
            {
                // currentWaypoint and nextWaypoint are in the same region
                if (0 == currentNavigationStep)
                {
                    // first waypoint from the beginning
                    // need to refine the turndirection in this case
                    information._turnDirection = TurnDirection.FirstDirection;
                    Tuple<WaypointEdge, string> WaypointEdgeTuple =
                        GetWaypointEdgeInRegion(currentRegionID,
                                                currentWaypointID,
                                                nextWaypointID);
                    WaypointEdge currentEdge = WaypointEdgeTuple.Item1;

                    SetInformationByWaypointEdge(ref information,
                        currentEdge, WaypointEdgeTuple.Item2);
                }
                else
                {
                    if (!previousRegionID.Equals(currentRegionID))
                    {
                        // currentWaypoint and nextWaypoint are in the same region
                        // previouWaypoint and currentWaypoint are acrss regions
                        if (!_regions[previousRegionID]._floor.Equals(
                            _regions[currentRegionID]._floor))
                        {
                            // previousWaypoint and currentWaypoint are on 
                            // different floor need to refine the turndirection
                            // in this case
                            information._turnDirection =
                                TurnDirection.FirstDirection;
                            Tuple<WaypointEdge, string> WaypointEdgeTuple =
                                GetWaypointEdgeInRegion(currentRegionID,
                                                        currentWaypointID,
                                                        nextWaypointID);
                            WaypointEdge currentEdge =
                                WaypointEdgeTuple.Item1;

                            SetInformationByWaypointEdge(ref information,
                                currentEdge, WaypointEdgeTuple.Item2);
                        }
                        else
                        {
                            // previousWaypoint and currentWaypoint are on the 
                            // same floor
                            RegionEdge prevEdge =
                                GetRegionEdgeMostNearSourceWaypoint
                                (previousRegionID,
                                previousWaypointID,
                                currentRegionID,
                                avoidConnectionTypes);


                            Tuple<WaypointEdge, string> WaypointEdgeTuple =
                                GetWaypointEdgeInRegion(currentRegionID,
                                                        currentWaypointID,
                                                        nextWaypointID);

                            WaypointEdge currentEdge =
                                WaypointEdgeTuple.Item1;

                            information._turnDirection =
                                GetTurnDirection(currentEdge._direction, prevEdge._direction);

                            information._connectionType =
                                currentEdge._connectionType;
                            information._distance =
                                Convert.ToInt32(currentEdge._distance);

                            information._directionPicture =
                                WaypointEdgeTuple.Item2;
                        }
                    }
                    else
                    {
                        // currentWaypoint and nextWaypoint are in the same 
                        // region previousWaypoint and currentWaypoint are in 
                        // the same region

                        WaypointEdge prevEdge = GetWaypointEdgeInRegion
                            (previousRegionID,
                             previousWaypointID,
                             currentWaypointID).Item1;

                        Tuple<WaypointEdge, string> WaypointEdgeTuple =
                            GetWaypointEdgeInRegion(currentRegionID,
                            currentWaypointID,
                            nextWaypointID);

                        WaypointEdge currentEdge = WaypointEdgeTuple.Item1;

                        information._turnDirection =
                            GetTurnDirection(currentEdge._direction, prevEdge._direction);
                        information._connectionType =
                            currentEdge._connectionType;
                        information._distance =
                            Convert.ToInt32(currentEdge._distance);
                        information._directionPicture =
                            WaypointEdgeTuple.Item2;
                    }
                }
            }
            return information;
        }

        private TurnDirection GetTurnDirection(CardinalDirection currentDirection,
            CardinalDirection prevDirection)
        {
            if (currentDirection - prevDirection >= 0)
                return (TurnDirection)(currentDirection - prevDirection);
            return (TurnDirection)(currentDirection - prevDirection + 8);
        }

        private void SetInformationByRegionEdge(ref InstructionInformation information,
            RegionEdge edge, string directionPicture)
        {
            information._relatedDirectionOfFirstDirection = edge._direction;
            information._connectionType = edge._connectionType;
            information._distance = Convert.ToInt32(edge._distance);
            information._directionPicture = directionPicture;
        }

        private void SetInformationByWaypointEdge(ref InstructionInformation information,
            WaypointEdge waypointEdge, string directionPicture)
        {
            information._distance = Convert.ToInt32(waypointEdge._distance);
            information._directionPicture = directionPicture;
            information._relatedDirectionOfFirstDirection = waypointEdge._direction;
            information._connectionType = waypointEdge._connectionType;
        }

        public Graph<Guid, string> GenerateRegionGraph
            (ConnectionType[] avoidConnectionTypes)
        {
            Graph<Guid, string> graph = new Graph<Guid, string>();

            foreach (KeyValuePair<Guid, Region> regionItem in _regions)
            {
                graph.AddNode(regionItem.Key);
            }

            foreach (KeyValuePair<Tuple<Guid, Guid>,
                List<RegionEdge>> regionEdgeItem in _edges)
            {
                Guid node1 = regionEdgeItem.Key.Item1;
                Guid node2 = regionEdgeItem.Key.Item2;

                uint node1Key = graph.Where(node => node.Item.Equals(node1))
                                .Select(node => node.Key).First();
                uint node2Key = graph.Where(node => node.Item.Equals(node2))
                                .Select(node => node.Key).First();

                int node1EdgeDistance = int.MaxValue;
                int node1EdgeIndex = -1;
                int node2EdgeDistance = int.MaxValue;
                int node2EdgeIndex = -1;
                for (int i = 0; i < regionEdgeItem.Value.Count(); i++)
                {
                    RegionEdge edgeItem = regionEdgeItem.Value[i];
                    if (!avoidConnectionTypes.Contains(edgeItem._connectionType))
                    {

                        if (DirectionalConnection.BiDirection ==
                            edgeItem._biDirection ||
                            (DirectionalConnection.OneWay ==
                            edgeItem._biDirection && 1 == edgeItem._source))
                        {
                            int edgeDistance =
                                Convert.ToInt32(edgeItem._distance);
                            if (edgeDistance < node1EdgeDistance)
                            {
                                node1EdgeDistance = edgeDistance;
                                node1EdgeIndex = i;
                            }
                        }

                        if (DirectionalConnection.BiDirection ==
                            edgeItem._biDirection ||
                        (DirectionalConnection.OneWay ==
                        edgeItem._biDirection && 2 == edgeItem._source))
                        {
                            int edgeDistance =
                                Convert.ToInt32(edgeItem._distance);
                            if (edgeDistance < node2EdgeDistance)
                            {
                                node2EdgeDistance = edgeDistance;
                                node2EdgeIndex = i;
                            }
                        }

                    }
                }
                if (-1 != node1EdgeIndex)
                {
                    graph.Connect
                        (node1Key, node2Key, node1EdgeDistance, string.Empty);
                }
                if (-1 != node2EdgeIndex)
                {
                    graph.Connect
                        (node2Key, node1Key, node2EdgeDistance, string.Empty);
                }
            }
            return graph;
        }

        public Graph<Guid, string> GenerateNavigraph(Guid regionID,
            ConnectionType[] avoidConnectionTypes)
        {
            Graph<Guid, string> graph = new Graph<Guid, string>();

            foreach (KeyValuePair<Guid, Waypoint> waypointItem
                     in _navigraphs[regionID]._waypoints)
            {
                graph.AddNode(waypointItem.Key);
            }

            foreach (KeyValuePair<Tuple<Guid, Guid>, WaypointEdge>
                waypointEdgeItem in _navigraphs[regionID]._edges)
            {
                Guid node1 = waypointEdgeItem.Key.Item1;
                Guid node2 = waypointEdgeItem.Key.Item2;
                uint node1Key = graph.Where(node => node.Item.Equals(node1))
                                  .Select(node => node.Key).First();
                uint node2Key = graph.Where(node => node.Item.Equals(node2))
                                  .Select(node => node.Key).First();

                // should refine distance, bi-direction, direction, connection
                // type later
                int distance = int.MaxValue;
                Tuple<Guid, Guid> edgeKey =
                    new Tuple<Guid, Guid>(node1, node2);
                WaypointEdge edgeItem = _navigraphs[regionID]._edges[edgeKey];
                if (!avoidConnectionTypes.Contains(edgeItem._connectionType))
                {
                    distance = System.Convert.ToInt32(edgeItem._distance);

                    if (DirectionalConnection.BiDirection ==
                        edgeItem._biDirection)
                    {
                        // Graph.Connect is on-way, not bi-drectional
                        graph.Connect(node1Key, node2Key, distance,
                            string.Empty);
                        graph.Connect(node2Key, node1Key, distance,
                            string.Empty);
                    }
                    else if (DirectionalConnection.OneWay ==
                        edgeItem._biDirection)
                    {
                        if (1 == edgeItem._source)
                        {
                            graph.Connect(node1Key, node2Key, distance,
                                string.Empty);
                        }
                        else if (2 == edgeItem._source)
                        {
                            graph.Connect(node2Key, node1Key, distance,
                                string.Empty);
                        }
                    }
                }
            }

            return graph;
        }

        public List<IPSType> GetUsedIPSTyps()
        {
            HashSet<IPSType> type = new HashSet<IPSType>();
            foreach (Guid regionID in GetAllRegionIDs())
                type.Add(GetRegionIPSType(regionID));

            return type.ToList();
        }
    }


    #region Enums and Structures
    public struct InstructionInformation
    {
        public TurnDirection _turnDirection { get; set; }
        public TurnDirection _nextDirection { get; set; }
        public CardinalDirection _relatedDirectionOfFirstDirection
        { get; set; }
        public ConnectionType _connectionType { get; set; }
        public int _floor { get; set; }
        public string _regionName { get; set; }
        public int _distance { get; set; }
        public string _directionPicture { get; set; }
        public bool _isVirtualWay { get; set; }
    }

    public enum LocationType
    {
        landmark = 0,
        junction, //junction_branch, 
        midpath,
        terminal, //terminal_destination,
        portal
    }

    public enum CardinalDirection
    {
        North = 0,
        Northeast,
        East,
        Southeast,
        South,
        Southwest,
        West,
        Northwest
    }

    public enum TurnDirection
    {
        Null = -2,  // for Portal type combination instruction.
        FirstDirection = -1, // Exception: used only in the first step
        Forward = 0,
        Forward_Right,
        Right,
        Backward_Right,
        Backward,
        Backward_Left,
        Left,
        Forward_Left,
        Up,
        Down
    }

    public enum IPSType
    {
        LBeacon = 0,
        iBeacon,
        GPS
    }

    public enum DirectionalConnection
    {
        OneWay = 1,
        BiDirection = 2
    }

    public enum ConnectionType
    {
        NormalHallway = 0,
        Stair,
        Elevator,
        Escalator,
        VirtualHallway
    }

    public enum CategoryType
    {
        Others = 0,
        Clinics,
        Cashier,
        Exit,
        ExaminationRoom,
        Pharmacy,
        ConvenienceStore,
        Bathroom,
        BloodCollectionCounter,
        Elevator,
        Parking,
        Office,
        ConferenceRoom,
        Stair,
        Escalator,
        CommunityHealth,  //健檢中心
        Radiology,        //放射檢查
        HealthEducation, //衛教室
        Procedures //處置
    }
    #endregion
}
