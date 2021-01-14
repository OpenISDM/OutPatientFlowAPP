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
 *      1.0.0, 201911125
 * 
 * File Name:
 *
 *      FirstDirectionInstruction.cs
 *
 * Abstract:
 *
 *      This file used to get the first direction information. When the user 
 *      first uses our APP They do not know where there are and which direction
 *      they should face.
 *      FirstDirectionInstruction can tell the user their nearest landmark that
 *      they should face to.
 *      
 * Authors:
 *
 *      Eric Lee, ericlee@iis.sinica.edu.tw
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace IndoorNavigation.Models.NavigaionLayer
{
    public class FirstDirectionInstruction
    {
        private Dictionary<Guid, string> _landmark;
        private Dictionary<Guid, CardinalDirection> _relatedDirection;
        private Dictionary<Guid, int> _faceOrBack;
        private Dictionary<Tuple<Guid, Guid>, string> _specialString;

        public FirstDirectionInstruction(XmlDocument fileName)
        {
            Console.WriteLine(">>FirstDirectionInstruction constructor");
            _landmark = new Dictionary<Guid, string>();
            _relatedDirection = new Dictionary<Guid, CardinalDirection>();
            _faceOrBack = new Dictionary<Guid, int>();
            _specialString = new Dictionary<Tuple<Guid, Guid>, string>();

            XmlNodeList xmlWaypoint =
                fileName.SelectNodes("first_direction_XML/waypoint");

            foreach (XmlNode xmlNode in xmlWaypoint)
            {
                XmlElement xmlElement = (XmlElement)xmlNode;
                string tempLandmark =
                    xmlElement.GetAttribute("Landmark").ToString();
                CardinalDirection tempRelatedDirection =
                    (CardinalDirection)Enum.Parse(typeof(CardinalDirection),
                    xmlElement.GetAttribute("RelatedDirection"),
                    false);
                int tempFaceOrBack = 
                    int.Parse(xmlElement.GetAttribute("FaceOrBack"));

                string waypointIDs = xmlElement.GetAttribute("id");
                string[] arrayWaypointIDs = waypointIDs.Split(';');

                for (int i = 0; i < arrayWaypointIDs.Count(); i++)
                {
                    Guid waypointID = Guid.Parse(arrayWaypointIDs[i]);
                    _landmark.Add(waypointID, tempLandmark);
                    _relatedDirection.Add(waypointID, tempRelatedDirection);
                    _faceOrBack.Add(waypointID, tempFaceOrBack);
                }

            }

            Console.WriteLine(">>Read Special string part");
            XmlNodeList specialStringNode =
                fileName.SelectNodes("first_direction_XML/specials/special");

            foreach (XmlNode node in specialStringNode)
            {
                Guid source =
                    new Guid(node.Attributes["source"].Value);

                Guid destination =
                    new Guid(node.Attributes["destination"].Value);

                string specialString = node.Attributes["instruction"].Value;

                Tuple<Guid, Guid> pair =
                    new Tuple<Guid, Guid>(source, destination);

                _specialString.Add(pair, specialString);
            }
        }

        public string GetSpecialString(Guid source, Guid destination)
        {
            return _specialString[new Tuple<Guid, Guid>(source, destination)];
        }

        public string returnLandmark(Guid currentGuid)
        {
            return _landmark[currentGuid];
        }
        public CardinalDirection returnDirection(Guid currentGuid)
        {
            return _relatedDirection[currentGuid];
        }
        public int returnFaceOrBack(Guid currentGuid)
        {
            return _faceOrBack[currentGuid];
        }
    }

    public enum InitialDirection
    {
        Face = 0,
        Back = 1
    }
}
