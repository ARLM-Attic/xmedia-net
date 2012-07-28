var map;
var infowindow = null;
var mapCenter;

var latLng = Array();
var marker = Array();
var infowindow = Array();
var contentString = Array();
var mapRosterItemList = Array();
var mapRosterItemHash = Array();
var elevator;
var mapOptions;

var latLngBounds;
var boundBox;
var southWest;
var southEast;
var northWest;
var northEast;
var north;
var south;
var east;
var west;
var lngSpan;
var latSpan;
var bDebug = false;
var variables = Array();
var variableHash = Array();
var primaryBareJID;
var mapAnnotationList = Array();
var mapAnnotationHash = Array();
var mapManager = new MapManager();

var MarkerInfoWindowStyleType = {
    "MarkerOnly": 0,
    "InfoWindowWithAvatarOnly": 1,
    "InfoWindowWithAvatarAndText": 2,
    "InfoWindowWithTextOnly": 3
};


// Load the Visualization API and the columnchart package.
google.load("visualization", "1", { packages: ["columnchart"] });

function MapManager() {
    var center;
    var buddyToFollow;
    var bCenter = true;

}

function insertText(val, e) {
    document.getElementById(e).innerHTML += val;
}

function initialize() {
    elevator = new google.maps.ElevationService();
    var mapDiv = document.getElementById('map-canvas');
    map = new google.maps.Map(mapDiv, {
        center: new google.maps.LatLng(32, -97),
        zoom: 15,
        mapTypeId: google.maps.MapTypeId.ROADMAP
    });
}

// -----------------
// Utility Functions
// -----------------

function pick(arg, def) {
    return (typeof arg == 'undefined' ? def : arg);
}

function varExists(arg) {


}

function setAsPrimaryJID() {

}

function contains(a, obj) {
    var i = a.length;
    while (i--) {
        if (a[i] === obj) {
            return true;
        }
    }
    return false;
}

function DumpMapAnnotations() {
    alert('==> entering DumpMapAnnotations()');
    if (mapAnnotationHash != null) {
        for (key in mapAnnotationHash) {
            alert("Hash item: " + mapAnnotation);
            alert("Hash item: " + mapAnnotation[key]);
            alert(mapAnnotationHash['test2@ninethumbs.com']);

            alert(mapAnnotationHash['test2@ninethumbs.com'][key]);
        }
    } else {
        alert("mapAnnotationHash is null");
    }

    if (mapAnnotationList != null) {
        for (mapAnnotation in mapAnnotationList) {
            alert("List item: " + mapAnnotation);
        }
    } else {
        alert("mapAnnotationList is null");
    }
    alert('<== leaving DumpMapAnnotations()');
}






// -----------------
// Class Definitions
// -----------------  
function JID(_User, _Domain, _Resource, _FullJID, _BareJID) {
    var User = pick(_User, '');
    var Domain = pick(_Domain, '');
    var Resource = pick(_Resource, '');
    var FullJID = pick(_FullJID, '');
    var BareJID = pick(_BareJID, '');

    function ToString() {
        return FullJID;
    }
}

function GeoLoc(_lat, _lon, _TimeStamp, _bIsDirty) {
    var lat = pick(_lat, 0.0);
    var lon = pick(_lon, 0.0);
    var TimeStamp = pick(_TimeStamp, new Date());
    var bIsDirty = pick(_bIsDirty, false);
    var latLonString = lat + ", " + lon;

    function ToString() {
        latLonString = lat + ", " + lon;
        return latLonString;
    }
}

function RosterItem(_JID, _Name, _GeoLoc, _PresenceStatus, _Group, _AvatarImagePath, _HasNewMessages) {
    var JID = pick(_JID, new JID());
    var Name = pick(_Name, '');
    var GeoLoc = pick(_GeoLoc, new GeoLoc());
    var PresenceStatus = pick(_PresenceStatus, new PresenceStatus());
    var Group = pick(_Group, '');
    var AvatarImagePath = pick(_AvatarImagePath, '');
    var HasNewMessages = pick(_HasNewMessages, false);

    function ToString() {
        return Name;
    }
}

function MapRosterItem(_RosterItem, _zIndex, _CurrentLocation, _PreviousLocation, _CurrentMilesPerHour,
                           _AverageMilesPerHour, _MarkerInfoWindowStyleType) {
    var RosterItem = pick(_RosterItem, new RosterItem());
    var zIndex = pick(_zIndex, 0);
    var CurrentLocation = pick(_CurrentLocation, new GeoLoc());
    var PreviousLocation = pick(_PreviousLocation, new GeoLoc());
    var CurrentMilesPerHour = pick(_CurrentMilesPerHour, 0.0);
    var AverageMilesPerHour = pick(_AverageMilesPerHour, 0.0);
    var MarkerInfoWindowStyleType = pick(_MarkerInfoWindowStyleType, MarkerInfoWindowStyleType.InfoWindowOnly);

    function ToString() {
        return RosterItem.ToString() + ' (' + RosterItem.GeoLoc.ToString() + ')';
    }

    function BuildContentString() {
        var retStr = '';
        return retStr;
    }
}

function MapAnnotation() {
    var marker = new google.maps.Marker();
    var latLng = new google.maps.LatLng();
    var infoWindow = new google.maps.InfoWindow();
    var title = "";
    var subtitle = "";
    var markerImage;
    var bInfoWindowOpen = false;
}

function buildInfoWindowContent(BareJID, name, lat, lng, avatarPath, bShowAvatar,
		            bShowActionBar, bShowJIDOnly, bUseGoogleLatitudeStyles) {
    var strRet = '';
    var timestamp = new Date();
    bShowAvatar = pick(bShowAvatar, false);
    bUseGoogleLatitudeStyles = pick(bUseGoogleLatitudeStyles, false);
    bShowActionBar = pick(bShowActionBar, false);
    bShowJIDOnly = pick(bShowJIDOnly, true);

    strRet += '<table>';
    strRet += '   <tr>';

    strRet += '      <td valign="top">';
    strRet += '         <table>';

    strRet += '   <tr>';
    strRet += '      <td valign="top"><b><font color="blue" size="+1">';
    strRet += name;
    strRet += '      </b></font></td>';
    strRet += '   </tr>';

    strRet += '   <tr>';
    strRet += '      <td valign="top"><font>';
    strRet += BareJID;
    strRet += '      </font></td>';
    strRet += '   </tr>';


    strRet += '   <tr>';
    strRet += '      <td valign="top"><font size="-1" color="gray">';
    strRet += timestamp;
    strRet += '      </font></td>';
    strRet += '</tr>';

    strRet += '</table>';

    if (bShowAvatar) {
        // 2nd column - blank space
        strRet += '<td width="30px">';
        strRet += '</td>';

        // 3rd column - avatar
        strRet += '<td>';
        strRet += '   <table>';
        strRet += '      <tr>';

        strRet += '         <td ><img width="70" src="';
        strRet += avatarPath;
        strRet += '"/>';
        strRet += '         </td>';
        strRet += '      </tr>';
        strRet += '   </table>';
        strRet += '</td>';

        // 4th column - blank space
        strRet += '<td width="15px">';
        strRet += '</td>';

    }

    strRet += '   </tr>';
    strRet += '</table>';

    if (bShowActionBar) {
        strRet += '<hr>';
    }

    return strRet;

    strRet = '<table><tr><td><font family="Arial" size="+1"><b>';
    strRet += name;
    strRet += '</b></font></td></tr><tr><td>';
    strRet += BareJID;
    strRet += '</td></tr><tr><td><font size="-1">';
    strRet += timestamp;
    strRet += '</font></td></tr></table>';

    return strRet;
}

function debug(strMsg) {
    if (bDebug) {
        alert(strMsg);
        insertText(strMsg, 'textIns');
    }
}

function parseData() {
    //for parsing the data
    return JSON.parse(document.getElementById("<%= hfData.ClientID%>").value);
}

function show() {
    var parsedData = parseData();
    var key = document.getElementById('idName').value;
    var obj = parsedData[key];
    if (typeof (obj) != 'undefined') {
        // If key is found then display the details
        debug(obj.Name);
        //debug(obj.Age);
        //debug(obj.Salary);
    }
    else {
        // key not found
        debug('No data found for key ' + key);
    }
}

function addSerializedObjectToDictionary(strTypeName, strObjectJSONSerialized) {
    var parsedData;

    try {
        parseData(strObjectJSONSerialized);
    }
    catch (err) {
        alert("Error parsing object " + strObjectJSONSerialized);
        return false;
    }

    variables.push(parsedData);

}

function addSerializedObjectToArray(strTypeName, strObjectJSONSerialized) {
    var parsedData;

    try {
        parseData(strObjectJSONSerialized);
    }
    catch (err) {
        alert("Error parsing object " + strObjectJSONSerialized);
        return false;
    }

    variables.push(parsedData);

    //var key = document.getElementById('idName').value;
    //var obj = parsedData[key];
    //if (typeof (obj) != 'undefined') {
    // If key is found then display the details
    //    debug(obj.Name);
    //debug(obj.Age);
    //debug(obj.Salary);
    //}
    //else {
    // key not found
    //    debug('No data found for key ' + key);
    // }
}

function parseData(strObjectJSONSerialized) {
    return JSON.parse(strObjectJSONSerialized);
}

function addSerializedObject(strObjectJSONSerialized) {
    debug(strObjectJSONSerialized);
    var bSuccess = true;

    var parsedData;
    try {
        parsedData = parseData(strObjectJSONSerialized);
    }
    catch (err) {
        debug("Exception parsing serialized data [ " + strObjectJSONSerialized + "].\r\nException: " + err);
        bSuccess = false;
    }
    return bSuccess;

    //var key = document.getElementById('idName').value;
    //var obj = parsedData[key];
    //if (typeof (obj) != 'undefined') {
    // If key is found then display the details
    //    debug(obj.Name);
    //debug(obj.Age);
    //debug(obj.Salary);
    //}
    //else {
    // key not found
    //    debug('No data found for key ' + key);
    // }
}

function addSerializedMarker(strRosterItemJSONSerialized) {
    debug(strRosterItemJSONSerialized);

    var parsedData = JSON.parse(strRosterItemJSONSerialized);

    debug(parsedData);

    //var key = document.getElementById('idName').value;
    //var obj = parsedData[key];
    //if (typeof (obj) != 'undefined') {
    // If key is found then display the details
    //    debug(obj.Name);
    //debug(obj.Age);
    //debug(obj.Salary);
    //}
    //else {
    // key not found
    //    debug('No data found for key ' + key);
    // }
}

function addMarker(BareJID, name, lat, lng, timestamp, avatarPath, bShowInfoWindow, bSetAsCenter) {
    debug("=> entering addMarker for " + BareJID);
    bShowInfoWindow = pick(bShowInfoWindow, true);
    bSetAsCenter = pick(bSetAsCenter, false);

    // does mapAnnotation already exist in array?
    var mapAnnotation = new MapAnnotation();

    // add in markerStyleType later
    var markerStyleType = 0;

    latLng[BareJID] = new google.maps.LatLng(lat, lng);
    mapAnnotation.latLng = latLng[BareJID];

    contentString[BareJID] = buildInfoWindowContent(BareJID, name, lat, lng, avatarPath);

    try {
        infowindow[BareJID] = new google.maps.InfoWindow({
            content: contentString[BareJID],
            zindex: -2
        });

        mapAnnotation.infoWindow = infowindow[BareJID];

        var title = BareJID + '<br>' + latLng[BareJID] + '<br>' + timestamp;
        mapAnnotation.title = title;

        var markerImage = new google.maps.MarkerImage(avatarPath, new google.maps.Size(32, 32));
        mapAnnotation.markerImage = markerImage;

        marker[BareJID] = new google.maps.Marker({
            position: latLng[BareJID],
            map: map,
            title: title,
            zindex: -2,
            //icon: markerImage,
            animation: google.maps.Animation.DROP
        });

        mapAnnotation.marker = marker[BareJID];

        // RIGGED FOR NOW.
        mapCenter = latLng[BareJID];
        if (mapManager.bCenter) {
            mapManager.center = mapCenter;
            mapManager.bCenter = false;
        }

        if (bSetAsCenter)
            map.setCenter(mapAnnotation.latLng);
        // latLng[BareJID]);

        if (bShowInfoWindow) {
            mapAnnotation.infoWindow.open(map, marker[BareJID]);
        }
        else {
            mapAnnotation.marker.setMap(map);
        }

        //	marker[BareJID].open(map);

        google.maps.event.addListener(mapAnnotation.marker, 'click',
                    function () {
                        alert('just clicked marker... ' + mapAnnotation.marker.title + ": " + mapAnnotation.bInfoWindowOpen);

                        if (mapAnnotation.bInfoWindowOpen == false) {
                            mapAnnotation.infoWindow.open(map, mapAnnotation.marker);
                            mapAnnotation.bInfoWindowOpen = true;
                        } else {
                            mapAnnotation.infoWindow.close
                            mapAnnotation.bInfoWindowOpen = true;
                        }
                    });

        //                google.maps.event.addListener(infowindow[BareJID], 'dblclick', function () {
        //                    //map.setCenter(latLng[BareJID]);
        //                    map.panTo(latLng[BareJID]);
        //                    //infowindow[BareJID].open(map, marker[BareJID]);
        //                });
    }
    catch (err) {
        debug(err);
    }

    mapAnnotationHash[BareJID] = mapAnnotation;

    // alert("Created mapAnnotation: " + mapAnnotation);
    // alert("Appended to the mapAnnotationHash for " + BareJID + " as " + mapAnnotation[BareJID]);
    debug("=> leaving addMarker for " + BareJID);
}

function centerMap(BareJID) {
    debug("=> entering centerMap for " + BareJID);
    //mapCenter = latLng[BareJID];
    map.setCenter(mapCenter);
    debug("=> leaving centerMap for " + BareJID);
}

function setBounds() {

    debug("=> entering setBounds");
    latLngBounds = new google.maps.LatLngBounds();
    for (latLongitude in latLng) {
        latLngBounds.extend(latLongitude);
    }
    map.fitBounds(latLngBounds);
    map.setCenter(mapCenter);

    debug("=> leaving setBounds");
}

function centerMapOnLatLon(lat, lon) {
    mapCenter = new google.maps.LatLng(lat, lon);
    map.setCenter(mapCenter);
}


function codeLatLng(latLng) {
    var geocoder = new google.maps.Geocoder();
    geocoder.geocode({ 'latLng': latLng },
       function (results, status) {
           if (status == google.maps.GeocoderStatus.OK) {
               if (results[0]) {
                   return results[0];
               } else {
                   return "Geocoder failed due to: " + status;
               }
           }
       });
}


google.maps.event.addDomListener(window, 'load', initialize);
