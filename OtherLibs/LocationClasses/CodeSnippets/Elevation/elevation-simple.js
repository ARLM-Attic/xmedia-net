﻿var elevator;
var map;
var infowindow = new google.maps.InfoWindow();
var denali = new google.maps.LatLng(63.3333333, -150.5);

function initialize() {
  var myOptions = {
    zoom: 8,
    center: denali,
    mapTypeId: 'terrain'
  }
  map = new google.maps.Map(document.getElementById("map_canvas"), myOptions);

  // Create an ElevationService
  elevator = new google.maps.ElevationService();

  // Add a listener for the click event and call getElevation on that location
  google.maps.event.addListener(map, 'click', getElevation);
}

function getElevation(event) {

  var locations = [];

  // Retrieve the clicked location and push it on the array
  var clickedLocation = event.latLng;
  locations.push(clickedLocation);

  // Create a LocationElevationRequest object using the array's one value
  var positionalRequest = {
    'locations': locations
  }

  // Initiate the location request
  elevator.getElevationForLocations(positionalRequest, function(results, status) {
    if (status == google.maps.ElevationStatus.OK) {

      // Retrieve the first result
      if (results[0]) {

        // Open an info window indicating the elevation at the clicked position
        infowindow.setContent("The elevation at this point 
is " + results[0].elevation + " meters.");
        infowindow.setPosition(clickedLocation);
        infowindow.open(map);
      } else {
        alert("No results found");
      }
    } else {
      alert("Elevation service failed due to: " + status);
    }
  });
}