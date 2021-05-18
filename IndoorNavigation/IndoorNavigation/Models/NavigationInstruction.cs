using IndoorNavigation.Models.NavigaionLayer;
using System;
using System.Collections.Generic;
using System.Text;

namespace IndoorNavigation.Models
{
    public class NavigationInstruction
    {
        public string _currentWaypointName;

        public string _nextWaypointName;

        public double _progress;

        public string _progressBar;
        public InstructionInformation _information { get; set; }

        public Guid _currentWaypointGuid;

        public Guid _nextWaypointGuid;

        public Guid _currentRegionGuid;

        public Guid _nextRegionGuid;

        public Guid _previousRegionGuid;

        public int _turnDirectionDistance;

        #region for extend new type navigation graph.
        public string _currentInstruction;
        public string _currentDirection;

        #endregion
    }

    public class NavigationEventArgs : EventArgs
    {
        public NavigationResult _result { get; set; }
        public NavigationInstruction _nextInstruction { get; set; }

    }
    public enum NavigationResult
    {
        Run = 0,
        AdjustRoute,
        Arrival,
        NoRoute,
        ArriveVirtualPoint,
        ArrivaIgnorePoint
    }
}
