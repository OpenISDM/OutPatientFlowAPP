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
        private Dictionary<Guid, NavigationGraph> _navigraphs { get; set; }
        private Dictionary<Guid, Region> _regions { get; set; }
        private Dictionary<Tuple<Guid, Guid>, List<RegionEdge>> _edges { get; set; }

        private Graph<Guid, string> _graphRegionGraph;
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
            public double _distance { get; set; }
            public bool _isVirtualWay { get; set; }
            // for picture value.
            // it might not be required in new navigationGraph
            //public string _picture12 { get; set; }
            //public string _picture21 { get; set; }

            // some stair, escalator or elevator can't be seen at previous 
            // point.
            // it might not be required in new navigationGraph
            //public bool _supportCombine { get; set; }
        }
        public struct RegionEdge
        {
            public Guid _region1 { get; set; }
            public Guid _region2 { get; set; }
            public Guid _waypoint1 { get; set; }
            public Guid _waypoint2 { get; set; }
            public int _source { get; set; }
            public DirectionalConnection _biDirection { get;set }
            public ConnectionType _connectionType
            { get; set; }
            public bool _isVirtualEdge { get; set; }
            public int _distance { get; set; }
        }
        #endregion
        public NavigationGraph_v2(XmlDocument xmlDocument)
        {

        }


        private RegionEdge GetRegionEdgeNearSourceWaypoint(Guid sourceRegionID, Guid sourceWaypointID, Guid sinkRegionID, ConnectionType[] avoidConnectionTypes)
        {
            return new RegionEdge();
        }

        private WaypointEdge GetWaypointEdge(Guid regionID, Guid sourceWaypointID, Guid sinkWaypointID)
        {
            return new WaypointEdge();
        }

        public Dictionary<Guid, Region> GetRegions()
        {
            return new Dictionary<Guid, Region>();
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

        public void GenerateRoute(Guid sourceRegionID, Guid sourceWaypointID, Guid destinationRegionID, Guid destinationWaypointID)
        {
            uint region1Key = _graphRegionGraph.Where(node=> node.Item.Equals(sourceRegionID)).Select(node=>node.Key).First();
            uint region2Key = _graphRegionGraph.Where(node=> node.Item.Equals(destinationRegionID)).Select(node=>node.Key).First();

            var pathRegions = _graphRegionGraph.Dijkstra(region1Key, region2Key).GetPath();

            

        }

        public void GenerateWrongWayDetect()
        {

        }

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
