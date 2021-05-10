using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using static IndoorNavigation.Utilities.Storage;
using static IndoorNavigation.Utilities.Helper;
using IndoorNavigation.Models.NavigaionLayer;
using Dijkstra.NET.Model;
using System.Linq;
using Dijkstra.NET.Extensions;
using static IndoorNavigation.Modules.Session;
using static IndoorNavigation.Utilities.Constants;
namespace IndoorNavigation.Models
{
    public class NavigationGraph_v2
    {
        #region Navi-Graph basic infos.
        private string _country;
        private string _cityCounty;
        private string _industryService;
        private string _ownerOrganization;
        private string _buildingName;
        private double _version;
        #endregion

        #region For generate route
        private Dictionary<Guid, Navigraph> _navigraphs { get; set; }
        private Dictionary<Guid, Region> _regions { get; set; }
        private Dictionary<Tuple<Guid, Guid>, List<RegionEdge>> _edges { get; set; }

        private Graph<Guid, string> _graphRegionGraph;
        private Dictionary<RegionWaypointPoint, List<RegionWaypointPoint>> _waypointsOnWrongWay;
        private List<RegionWaypointPoint> _waypointsOnRoute;
        private ConnectionType[] _avoidConnectionTypes;
        #endregion

        #region For multilingual
        #endregion

        #region Nest Structs and Classes
        public class Navigraph
        {
            public Guid _regionID { get; set; }
            public Dictionary<Guid, Waypoint> _waypoints { get; set; }
            public Dictionary<Tuple<Guid, Guid>, WaypointEdge> _edges { get; set; }

            // The key "Guid" is waypoint's Guid.
            // The value "List<Guid>" is beacons' Guid.
            public Dictionary<Guid, List<Guid>> _beacons { get; set; }
            // The key "Guid" is beacon's Guid.
            public Dictionary<Guid, int> _beaconRSSIThreshold { get; set; }
        }
        public struct WaypointEdge
        {
            public Guid _node1 { get; set; }
            public Guid _node2 { get; set; }
            public int _source { get; set; }
            public ConnectionType _connectionType { get; set; }
            public DirectionalConnection _biDirection { get; set; }
            public double _distance { get; set; }
            public bool _isVirtualWay { get; set; }
        }
        public struct RegionEdge
        {
            public Guid _region1 { get; set; }
            public Guid _region2 { get; set; }
            public Guid _waypoint1 { get; set; }
            public Guid _waypoint2 { get; set; }
            public int _source { get; set; }
            public DirectionalConnection _biDirection { get; set; }
            public ConnectionType _connectionType
            { get; set; }
            public bool _isVirtualEdge { get; set; }
            public double _distance { get; set; }
        }
        #endregion
        public NavigationGraph_v2(XmlDocument xmlDocument)
        {
            _regions = new Dictionary<Guid, Region>();
            _edges = new Dictionary<Tuple<Guid, Guid>, List<RegionEdge>>();
            _navigraphs = new Dictionary<Guid, Navigraph>();
            #region root basic attributes
            XmlElement navigationGraphElement =
                (XmlElement)xmlDocument.SelectSingleNode("navigation_graph");
            _country =
                navigationGraphElement.GetAttribute("country");
            _cityCounty =
                navigationGraphElement.GetAttribute("city_county");
            _industryService =
                navigationGraphElement.GetAttribute("industry_service");
            _ownerOrganization =
                navigationGraphElement.GetAttribute("owner_organization");
            _buildingName =
                navigationGraphElement.GetAttribute("building_name");
            _version =
                Convert.ToDouble(navigationGraphElement
                .GetAttribute("version"));
            #endregion

            #region Read navigation_graph/regions/region
            XmlNodeList xmlRegion =
                xmlDocument.SelectNodes("navigation_graph/regions/region");
            foreach (XmlNode regionNode in xmlRegion)
            {
                Region region = new Region();
                region._neighbors = new List<Guid>();
                region._waypointsByCategory =
                    new Dictionary<CategoryType, List<Waypoint>>();

                XmlElement xmlElement = (XmlElement)regionNode;
                region._id = Guid.Parse(xmlElement.GetAttribute("id"));

                region._IPSType =
                    (IPSType)Enum.Parse(typeof(IPSType),
                    xmlElement.GetAttribute("ips_type"),
                    false);
                region._name = xmlElement.GetAttribute("name");

                region._floor = int.Parse(xmlElement.GetAttribute("floor"));

                XmlNodeList xmlWaypoint = regionNode.SelectNodes("waypoint");
                foreach (XmlNode waypointNode in xmlWaypoint)
                {
                    Waypoint waypoint = new Waypoint();

                    waypoint._neighbors = new List<Guid>();
                    XmlElement xmlWaypointElement =
                        (XmlElement)waypointNode;

                    waypoint._id =
                        Guid.Parse(xmlWaypointElement.GetAttribute("id"));
                    waypoint._name =
                        xmlWaypointElement.GetAttribute("name");
                    waypoint._type =
                        (LocationType)Enum.Parse(typeof(LocationType),
                        xmlWaypointElement.GetAttribute("type"), false);
                    waypoint._lon =
                        double.Parse(xmlWaypointElement.GetAttribute("lon"));
                    waypoint._lat =
                        double.Parse(xmlWaypointElement.GetAttribute("lat"));
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
            XmlNodeList xmlRegionEdge = xmlDocument.SelectNodes("navigation_graph/regions/edge");
            foreach (XmlNode regionEdgeNode in xmlRegionEdge)
            {
                RegionEdge regionEdge = new RegionEdge();
                XmlElement xmlElement = (XmlElement)regionEdgeNode;
                regionEdge._region1 =
                    Guid.Parse(xmlElement.GetAttribute("region1"));
                regionEdge._waypoint1 =
                    Guid.Parse(xmlElement.GetAttribute("waypoint1"));
                regionEdge._region2 =
                    Guid.Parse(xmlElement.GetAttribute("region2"));
                regionEdge._waypoint2 =
                    Guid.Parse(xmlElement.GetAttribute("waypoint2"));
                regionEdge._biDirection =
                    (DirectionalConnection)Enum
                    .Parse(typeof(DirectionalConnection),
                    xmlElement.GetAttribute("bi_direction"),
                    false);

                regionEdge._source = 0;
                if (!string.IsNullOrEmpty(xmlElement.GetAttribute("source")))
                {
                    regionEdge._source =
                        int.Parse(xmlElement.GetAttribute("source"));
                }

                if (xmlElement.HasAttribute("isvirtualEdge"))
                {
                    regionEdge._isVirtualEdge =
                        XmlConvert.ToBoolean
                        (xmlElement.GetAttribute("isvirtualEdge"));
                }

                regionEdge._connectionType =
                    (ConnectionType)Enum.Parse(typeof(ConnectionType),
                    xmlElement.GetAttribute("connection_type"),
                    false);

                regionEdge._distance = 0;
                int distanceElevator = 3;
                int distanceEscalator = 5;
                int distanceStair = 7;

                switch (regionEdge._connectionType)
                {
                    case ConnectionType.Elevator:
                        regionEdge._distance = distanceElevator;
                        break;
                    case ConnectionType.Escalator:
                        regionEdge._distance = distanceEscalator;
                        break;
                    case ConnectionType.Stair:
                        regionEdge._distance = distanceStair;
                        break;
                    case ConnectionType.NormalHallway:
                    case ConnectionType.VirtualHallway:
                        bool foundNode1 = false;
                        double node1Lon = 0;
                        double node1Lat = 0;
                        bool foundNode2 = false;
                        double node2Lon = 0;
                        double node2Lat = 0;
                        foreach (KeyValuePair<CategoryType, List<Waypoint>> categoryItem in _regions[regionEdge._region1]._waypointsByCategory)
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

                        break;
                }

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
            XmlNodeList xmlNavigraph =
                xmlDocument.SelectNodes("navigation_graph/navigraphs/navigraph");
            foreach (XmlNode navigraphNode in xmlNavigraph)
            {
                Navigraph navigraph = new Navigraph();

                navigraph._waypoints = new Dictionary<Guid, Waypoint>();
                navigraph._edges = new Dictionary<Tuple<Guid, Guid>, WaypointEdge>();
                navigraph._beacons = new Dictionary<Guid, List<Guid>>();
                navigraph._beaconRSSIThreshold = new Dictionary<Guid, int>();
                XmlElement xmlElement = (XmlElement)navigraphNode;
                navigraph._regionID = Guid.Parse(xmlElement.GetAttribute("region_id"));

                XmlNodeList xmlWaypoint = navigraphNode.SelectNodes("waypoint");
                foreach (XmlNode waypointNode in xmlWaypoint)
                {
                    Waypoint waypoint = new Waypoint();
                    waypoint._neighbors = new List<Guid>();

                    XmlElement xmlWaypointElement = (XmlElement)waypointNode;
                    waypoint._id =
                        Guid.Parse(xmlWaypointElement.GetAttribute("id"));

                    waypoint._name = xmlWaypointElement.GetAttribute("name");
                    waypoint._type =
                        (LocationType)Enum.Parse(typeof(LocationType),
                        xmlWaypointElement.GetAttribute("type"), false);
                    waypoint._lon =
                        double.Parse(xmlWaypointElement.GetAttribute("lon"));
                    waypoint._lat =
                        double.Parse(xmlWaypointElement.GetAttribute("lat"));

                    waypoint._category =
                        (CategoryType)Enum.Parse(typeof(CategoryType),
                        xmlWaypointElement.GetAttribute("category"), false);
                    navigraph._waypoints.Add(waypoint._id, waypoint);
                }
                #endregion

                #region read edges block
                XmlNodeList xmlWaypointEdge =
                    navigraphNode.SelectNodes("edge");
                foreach (XmlNode waypointEdgeNode in xmlWaypointEdge)
                {
                    WaypointEdge waypointEdge = new WaypointEdge();
                    XmlElement xmlEdgeElement = (XmlElement)waypointEdgeNode;
                    waypointEdge._node1 =
                        Guid.Parse(xmlEdgeElement.GetAttribute("node1"));
                    waypointEdge._node2 =
                        Guid.Parse(xmlEdgeElement.GetAttribute("node2"));

                    waypointEdge._biDirection =
                        (DirectionalConnection)Enum.Parse
                        (typeof(DirectionalConnection),
                        xmlEdgeElement.GetAttribute("bi_direction"),
                        false);

                    waypointEdge._source = 0;
                    if (!string.IsNullOrEmpty
                        (xmlEdgeElement.GetAttribute("source")))
                    {
                        waypointEdge._source =
                            int.Parse(xmlEdgeElement.GetAttribute("source"));
                    }

                    waypointEdge._connectionType =
                        (ConnectionType)Enum.Parse(typeof(ConnectionType),
                                                   xmlEdgeElement
                                                   .GetAttribute
                                                   ("connection_type"),
                                                   false);

                    waypointEdge._distance =
                        GetDistance(navigraph._waypoints[waypointEdge._node1]._lon,
                                    navigraph._waypoints[waypointEdge._node1]._lat,
                                    navigraph._waypoints[waypointEdge._node2]._lon,
                                    navigraph._waypoints[waypointEdge._node2]._lat);

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

                #region read beacons
                XmlNodeList xmlBeacon = navigraphNode.SelectNodes("beacons/beacon");
                foreach (XmlNode beaconNode in xmlBeacon)
                {
                    XmlElement xmlBeaconElement = (XmlElement)beaconNode;
                    Guid beaconGuid = Guid.Parse(xmlBeaconElement.GetAttribute("uuid"));

                    string waypointIDs = xmlBeaconElement.GetAttribute("waypoint_ids");

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

        private double GetDistance(double lon1, double lat1, double lon2, double lat2)
        {
            double radLat1 = Rad(lat1);
            double radLng1 = Rad(lon1);
            double radLat2 = Rad(lat2);
            double radLng2 = Rad(lon2);
            double a = radLat1 - radLat2;
            double b = radLng1 - radLng2;
            return
                2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) +
                Math.Cos(radLat1) * Math.Cos(radLat2) *
                Math.Pow(Math.Sin(b / 2), 2))) * EARTH_RADIUS;
        }
        private double Rad(double d)
        {
            return (double)d * Math.PI / 180d;
        }

        private bool isConnectable(RegionEdge edge, int source)
        {
            return edge._biDirection == DirectionalConnection.BiDirection ||
                (edge._biDirection == DirectionalConnection.OneWay && edge._source == source);
        }

        private int GetMostCloseRegionEdge(Waypoint sourceWaypoint, Guid sourceRegionID, Tuple<Guid, Guid> edgeKey, int source)
        {
            if (!_edges.ContainsKey(edgeKey)) return -1;
            int index = 0;
            int minimumDistance = int.MaxValue;
            for (int i = 0; i < _edges[edgeKey].Count(); i++)
            {
                RegionEdge edgeItem = _edges[edgeKey][i];
                if (!_avoidConnectionTypes.Contains(edgeItem._connectionType))
                {
                    if (isConnectable(edgeItem, source))
                    {
                        Waypoint sinkWaypoint;
                        switch (source)
                        {
                            case 1:
                                sinkWaypoint = _navigraphs[sourceRegionID]._waypoints[edgeItem._waypoint1];
                                break;
                            case 2:
                                sinkWaypoint = _navigraphs[sourceRegionID]._waypoints[edgeItem._waypoint2];
                                break;
                            default:
                                throw new Exception("Error setting, please check the configs");
                        }

                        int edgeDistance =
                            (int)GetDistance(sourceWaypoint._lon
                            , sourceWaypoint._lat
                            , sinkWaypoint._lon
                            , sinkWaypoint._lat);

                        if (edgeDistance < minimumDistance)
                        {
                            minimumDistance = edgeDistance;
                            index = i;
                        }
                    }
                }
            }
            return index;
        }
        private RegionEdge GetRegionEdgeNearSourceWaypoint(Guid sourceRegionID, Guid sourceWaypointID, Guid sinkRegionID)
        {
            RegionEdge regionEdgeItem = new RegionEdge();

            Waypoint sourceWaypoint =
                _navigraphs[sourceRegionID]._waypoints[sourceWaypointID];

            // compare the normal case (R1, R2)
            Tuple<Guid, Guid> edgeKeyFromNode1 =
                new Tuple<Guid, Guid>(sourceRegionID, sinkRegionID);

            int indexEdge = GetMostCloseRegionEdge(sourceWaypoint, sourceRegionID, edgeKeyFromNode1, 1);
            if (-1 != indexEdge)
            {
                regionEdgeItem = _edges[edgeKeyFromNode1][indexEdge];
                return regionEdgeItem;
            }

            // compare the reverse case (R2, R1) because normal case (R1, R2) 
            //cannot find regionEdge
            Tuple<Guid, Guid> edgeKeyFromNode2 =
                new Tuple<Guid, Guid>(sinkRegionID, sourceRegionID);
            GetMostCloseRegionEdge(sourceWaypoint, sourceRegionID, edgeKeyFromNode2, 2);
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

                regionEdgeItem._connectionType =
                    _edges[edgeKeyFromNode2][indexEdge]._connectionType;
                return regionEdgeItem;
            }
            return regionEdgeItem;
        }

        private WaypointEdge GetWaypointEdge(Guid regionID, Guid sourceWaypointID, Guid sinkWaypointID)
        {
            WaypointEdge waypointEdge = new WaypointEdge();

            Tuple<Guid, Guid> edgeKeyFromNode1 =
                new Tuple<Guid, Guid>(sourceWaypointID,
                sinkWaypointID);
            Tuple<Guid, Guid> edgeKeyFromNode2 =
                new Tuple<Guid, Guid>(sinkWaypointID, sourceWaypointID);
            if (_navigraphs[regionID]._edges.ContainsKey(edgeKeyFromNode1))
            {
                // XML file contains (W1, W2) and the query input is 
                // (W1, W2) as well.
                waypointEdge = _navigraphs[regionID]._edges[edgeKeyFromNode1];
            }
            else if (_navigraphs[regionID]._edges.ContainsKey
                (edgeKeyFromNode2))
            {
                // XML file contains (W1, W2) but the query string is (W2, W1).
                waypointEdge = _navigraphs[regionID]._edges[edgeKeyFromNode2];
            }
            return waypointEdge;
        }

        public Graph<Guid, string> GenerateRegionGraph(ConnectionType[] avoidConnectionTypes)
        {
            Graph<Guid, string> graph = new Graph<Guid, string>();

            foreach (KeyValuePair<Guid, Region> regionItem in _regions)
            {
                graph.AddNode(regionItem.Key);
            }

            foreach (KeyValuePair<Tuple<Guid, Guid>, List<RegionEdge>> regionEdgeItem in _edges)
            {
                Guid node1 = regionEdgeItem.Key.Item1;
                Guid node2 = regionEdgeItem.Key.Item2;

                uint node1Key = graph.Where(node => node.Item.Equals(node1)).Select(node => node.Key).First();

                uint node2Key = graph.Where(node => node.Item.Equals(node2)).Select(node => node.Key).First();

                int node1EdgeDistance = int.MaxValue;
                int node1EdgeIndex = -1;
                int node2EdgeDistance = int.MaxValue;
                int node2EdgeIndex = -1;
                for (int i = 0; i < regionEdgeItem.Value.Count(); i++)
                {
                    RegionEdge edgeItem = regionEdgeItem.Value[i];
                    if (!avoidConnectionTypes.Contains(edgeItem._connectionType))
                    {

                        if (isConnectable(edgeItem, 1))
                        {
                            int edgeDistance =
                                Convert.ToInt32(edgeItem._distance);
                            if (edgeDistance < node1EdgeDistance)
                            {
                                node1EdgeDistance = edgeDistance;
                                node1EdgeIndex = i;
                            }
                        }

                        if (isConnectable(edgeItem, 1))
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

        public void GenerateRoute(Guid sourceRegionID, Guid sourceWaypointID, Guid destinationRegionID, Guid destinationWaypointID)
        {
            uint region1Key = _graphRegionGraph.Where(node => node.Item.Equals(sourceRegionID)).Select(node => node.Key).First();
            uint region2Key = _graphRegionGraph.Where(node => node.Item.Equals(destinationRegionID)).Select(node => node.Key).First();

            var pathRegions = _graphRegionGraph.Dijkstra(region1Key, region2Key).GetPath();

            if (0 == pathRegions.Count())
            {
                Console.WriteLine("No path.Need to change avoid connection" +
                                  " type");

                // TODO 修改"沒有路徑"的處理方式
                //_event.OnEventCall(new NavigationEventArgs
                //{
                //    _result = NavigationResult.NoRoute
                //});
                return;
            }

            // store the generate Dijkstra path across regions
            List<Guid> regionsOnRoute = new List<Guid>();
            for (int i = 0; i < pathRegions.Count(); i++)
            {
                regionsOnRoute.Add
                    (_graphRegionGraph[pathRegions.ToList()
                    [i]].Item);
            }
            _waypointsOnRoute.Add(
                new RegionWaypointPoint
                {
                    _regionID = sourceRegionID,
                    _waypointID = sourceWaypointID
                });

            for (int i = 0; i < _waypointsOnRoute.Count(); i++)
            {
                RegionWaypointPoint checkPoint = _waypointsOnRoute[i];
                Console.WriteLine("check index = {0}, count = {1}, region {2}"
                    + " waypoint {3}",
                    i,
                    _waypointsOnRoute.Count(),
                    checkPoint._regionID,
                    checkPoint._waypointID);
                if (regionsOnRoute.IndexOf(checkPoint._regionID) + 1 <
                    regionsOnRoute.Count())
                {
                    LocationType waypointType =
                        GetWaypointType(checkPoint._regionID, checkPoint._waypointID);

                    Guid nextRegionID =
                        regionsOnRoute[regionsOnRoute.
                                       IndexOf(checkPoint._regionID) + 1];

                    ConnectionType[] _avoidConnectionTypes = new ConnectionType[0];
                    //TODO 將avoidConnectionType直接帶入navigationGraph.
                    PortalWaypoints portalWaypoints =
                        GetPortalWaypoints(checkPoint._regionID,
                        checkPoint._waypointID,
                        nextRegionID);

                    if (LocationType.portal != waypointType)
                    {
                        _waypointsOnRoute.Add(new RegionWaypointPoint
                        {
                            _regionID = checkPoint._regionID,
                            _waypointID =
                            portalWaypoints._portalWaypoint1
                        });
                    }
                    else if (LocationType.portal == waypointType)
                    {
                        if (!checkPoint._waypointID.Equals(portalWaypoints._portalWaypoint1))
                        {
                            _waypointsOnRoute.Add(new RegionWaypointPoint
                            {
                                _regionID = checkPoint._regionID,
                                _waypointID = portalWaypoints._portalWaypoint1
                            });
                        }
                        else
                        {
                            _waypointsOnRoute.Add(new RegionWaypointPoint
                            {
                                _regionID = nextRegionID,
                                _waypointID = portalWaypoints._portalWaypoint2
                            });
                        }
                    }
                }
            }
            int indexLastCheckPoint = _waypointsOnRoute.Count() - 1;
            if (!(destinationRegionID.
                Equals(_waypointsOnRoute[indexLastCheckPoint]._regionID) &&
                destinationWaypointID.
                Equals(_waypointsOnRoute[indexLastCheckPoint]._waypointID)))
            {
                _waypointsOnRoute.Add(new RegionWaypointPoint
                {
                    _regionID = destinationRegionID,
                    _waypointID = destinationWaypointID
                });
            }

            //TODO : remeber to implement "remove dumplicate waypoint"
        }

        public LocationType GetWaypointType(Guid regionID, Guid waypointID)
        {
            return _navigraphs[regionID]._waypoints[waypointID]._type;
        }

        public PortalWaypoints GetPortalWaypoints(Guid sourceRegionID, Guid sourceWaypointID, Guid sinkRegionID)
        {
            RegionEdge regionEdge =
                GetRegionEdgeNearSourceWaypoint(sourceRegionID, sourceWaypointID, sinkRegionID);

            return new PortalWaypoints(regionEdge._waypoint1, regionEdge._waypoint2);
        }

        public int getWaypointsCountOnRoute()
        {
            return _waypointsOnRoute.Count;
        }

        public List<Guid> GetAllRegionIDs()
        {
            return new List<Guid>(_regions.Keys);
        }

        public RegionWaypointPoint getWaypointOnRoute(int index)
        {
            return _waypointsOnRoute[index];
        }

        public List<RegionWaypointPoint> getWrongWaypoints(RegionWaypointPoint currentWaypoint)
        {
            return _waypointsOnWrongWay[_waypointsOnWrongWay];
        }

        public List<Guid> GetNeighbor(Guid regionID, Guid waypointID)
        {
            return _navigraphs[regionID]._waypoints[waypointID]._neighbors;
        }

        public void GenerateWrongWay()
        {
            int index = 1;
            _waypointsOnWrongWay = new Dictionary<RegionWaypointPoint, List<RegionWaypointPoint>>();

            foreach (RegionWaypointPoint currentWaypoint in _waypointsOnRoute)
            {
                if (GetWaypointType(currentWaypoint._regionID, currentWaypoint._waypointID) == LocationType.portal)
                {
                    AddPortalWrongWaypoint(currentWaypoint, index, currentWaypoint._waypointID);
                }

                foreach (Guid neighborWaypointID in GetNeighbor(currentWaypoint._regionID, currentWaypoint._waypointID))
                {
                    if (_waypointsOnRoute.Count > index)
                    {
                        if (_waypointsOnRoute[index]._waypointID != neighborWaypointID)
                        {
                            double distanceBetweenCurrentAndNeighbor =
                                GetDistanceBetweenWaypoints(currentWaypoint._regionID, currentWaypoint._waypointID, neighborWaypointID);
                            double distanceBetweenNextAndNeighbor;

                            if (currentWaypoint._regionID == _waypointsOnRoute[index]._regionID)
                                distanceBetweenNextAndNeighbor =
                                    GetDistanceBetweenWaypoints(currentWaypoint._regionID,
                                    _waypointsOnRoute[index]._waypointID, neighborWaypointID);
                            else
                                distanceBetweenNextAndNeighbor = distanceBetweenCurrentAndNeighbor;

                            if (distanceBetweenCurrentAndNeighbor >= WRONG_WAY_DISTANCE &&
                                distanceBetweenNextAndNeighbor >= WRONG_WAY_DISTANCE)
                            {
                                if (index < 2 || (index >= 2 && _waypointsOnRoute[index - 2]._waypointID != neighborWaypointID))
                                {
                                    AddWrongWaypoint(currentWaypoint,
                                        new RegionWaypointPoint(currentWaypoint._regionID, neighborWaypointID));
                                }
                            }
                            else if (distanceBetweenCurrentAndNeighbor < WRONG_WAY_DISTANCE)
                            {
                                AdditionLayerWrongWay(neighborWaypointID, currentWaypoint, index);
                            }

                            if(index >= 2 && _waypointsOnRoute[index-2]._waypointID == neighborWaypointID)
                            {
                                AdditionLayerWrongWay(neighborWaypointID, currentWaypoint, index);
                            }
                        }
                        else if (!_waypointsOnWrongWay.Keys.Contains(currentWaypoint))
                            _waypointsOnWrongWay.Add(currentWaypoint, new List<RegionWaypointPoint>());
                    }
                    else break;
                }
                index++;
            }
        }

        public IPSType GetRegionIPSType(Guid regionID)
        {
            return _regions[regionID]._IPSType;
        }

        public void GenerateIPSTable()
        {
            List<HashSet<IPSType>> IPSTable = new List<HashSet<IPSType>>();

            foreach (RegionWaypointPoint waypoint in _waypointsOnRoute)
            {
                HashSet<IPSType> subTabe = new HashSet<IPSType>();
                subTabe.Add(GetRegionIPSType(waypoint._regionID));

                try
                {
                    foreach (RegionWaypointPoint wrongWaypoint in _waypointsOnWrongWay[waypoint])
                    {

                        subTabe.Add(GetRegionIPSType(wrongWaypoint._regionID));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("generate ips table exception : " + e.Message);
                }
                IPSTable.Add(subTabe);
            }
        }
        public void AddPortalWrongWaypoint(RegionWaypointPoint currentWaypoint, int index, Guid neighborGuid)
        {
            foreach (Guid neighborRegionID in _regions[currentWaypoint._regionID]._neighbors)
            {
                RegionEdge connectRegionEdge =
                    GetRegionEdgeNearSourceWaypoint(currentWaypoint._regionID, neighborGuid, neighborRegionID);
                if (!isEmptyGuid(connectRegionEdge._waypoint2))
                {
                    if (_waypointsOnRoute.Count() > index)
                    {
                        if (_waypointsOnRoute[index]._waypointID != connectRegionEdge._waypoint2)
                        {
                            AddWrongWaypoint(currentWaypoint,
                              new RegionWaypointPoint(connectRegionEdge._region2, connectRegionEdge._waypoint2));
                        }
                        else
                        {
                            if (!_waypointsOnWrongWay.Keys.Contains(currentWaypoint))
                            {
                                _waypointsOnWrongWay.Add(currentWaypoint, new List<RegionWaypointPoint>());
                            }
                        }
                    }
                }
            }
        }
        public void AddWrongWaypoint(RegionWaypointPoint currentWaypoint, RegionWaypointPoint wrongWayWaypoint)
        {
            if (!_waypointsOnWrongWay.Keys.Contains(currentWaypoint))
            {
                _waypointsOnWrongWay.Add(currentWaypoint, new List<RegionWaypointPoint> { wrongWayWaypoint });
            }
            else
            {
                _waypointsOnWrongWay[currentWaypoint].Add(wrongWayWaypoint);
            }
        }
        //TODO the code is similar to GenerateWrongWay function, maybe could use one-time recursion to simply this part.
        public void AdditionLayerWrongWay(Guid neighborWaypointID, RegionWaypointPoint currentWaypoint, int index)
        {
            // when the neighbor type is portal, need to add wrong waypoint that next region.
            if (GetWaypointType(currentWaypoint._regionID, neighborWaypointID) == LocationType.portal)
            {
                //TODO remember to add parameter to this.
                AddPortalWrongWaypoint(currentWaypoint, index, neighborWaypointID);
            }

            //TODO the logic is the same in GenerateWrongWay function, just write one.
            foreach (Guid nextNeighborWaypointID in GetNeighbor(currentWaypoint._regionID, neighborWaypointID))
            {
                if (_waypointsOnRoute.Count() > index)
                {
                    double distanceBetweenCurrentAndNeighbor =
                        GetDistanceBetweenWaypoints(currentWaypoint._regionID, currentWaypoint._waypointID, nextNeighborWaypointID);
                    double distanceBetweenNextAndNearNeighbor;

                    if (currentWaypoint._regionID == _waypointsOnRoute[index]._regionID)
                        distanceBetweenNextAndNearNeighbor =
                            GetDistanceBetweenWaypoints(currentWaypoint._regionID, nextNeighborWaypointID, _waypointsOnRoute[index]._waypointID);
                    else
                        distanceBetweenNextAndNearNeighbor = distanceBetweenCurrentAndNeighbor;

                    if (_waypointsOnRoute[index]._waypointID != nextNeighborWaypointID &&
                        nextNeighborWaypointID != neighborWaypointID &&
                        distanceBetweenCurrentAndNeighbor >= WRONG_WAY_DISTANCE &&
                        distanceBetweenNextAndNearNeighbor >= WRONG_WAY_DISTANCE)
                    {
                        if (index < 2 ||
                              (index >= 2 && _waypointsOnRoute[index - 2]._waypointID != nextNeighborWaypointID))
                        {
                            AddWrongWaypoint(currentWaypoint,
                                  new RegionWaypointPoint(currentWaypoint._regionID,
                                  nextNeighborWaypointID));
                        }
                    }
                    else if (!_waypointsOnWrongWay.Keys.Contains(currentWaypoint))
                        _waypointsOnWrongWay.Add(currentWaypoint, new List<RegionWaypointPoint>());
                }
            }
        }

        public int GetBeaconRSSIThreshold(Guid regionGuid, Guid beaconUUID)
        {
            return _navigraphs[regionGuid]._beaconRSSIThreshold[beaconUUID];
        }
        public double GetDistanceBetweenWaypoints(Guid regionID, Guid waypointID1, Guid waypointID2)
        {
            double lat1 = _navigraphs[regionID]._waypoints[waypointID1]._lat;
            double lon1 = _navigraphs[regionID]._waypoints[waypointID1]._lon;
            double lat2 = _navigraphs[regionID]._waypoints[waypointID2]._lat;
            double lon2 = _navigraphs[regionID]._waypoints[waypointID2]._lon;
            return GetDistance(lon1, lat1, lon2, lat2);
        }

        //public InstructionInformation GetInstruction(RegionWaypointPoint previous, RegionWaypointPoint current, RegionWaypointPoint next, RegionWaypointPoint nextnext)
        //{

        //}
        #region Get NavigationGraph Attributes
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
    }

    public enum DirectionalConnection
    {
        OneWay = 1,
        BiDirection = 2,
    }
}
