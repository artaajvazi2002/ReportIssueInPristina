window.getCurrentLocation = function () {
  return new Promise((resolve, reject) => {
    if (!navigator.geolocation) {
      reject("Geolocation is not supported by this browser.");
      return;
    }

    navigator.geolocation.getCurrentPosition(
      function (position) {
        const latitude = position.coords.latitude;
        const longitude = position.coords.longitude;

        resolve({
          location: `Lat: ${latitude.toFixed(6)}, Lng: ${longitude.toFixed(6)}`,
          latitude,
          longitude,
        });
      },
      function () {
        reject("Unable to get your location.");
      },
    );
  });
};

window.initializeIssueMap = function (elementId, issues) {
  const mapElement = document.getElementById(elementId);
  if (!mapElement || !window.L) {
    return;
  }

  const pristinaBounds = L.latLngBounds([42.58, 21.08], [42.75, 21.3]);
  const map = L.map(mapElement, {
    maxBounds: pristinaBounds,
    maxBoundsViscosity: 1,
    minZoom: 11,
  }).setView([42.6629, 21.1655], 13);
  L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
    attribution: "&copy; OpenStreetMap contributors",
    maxZoom: 19,
  }).addTo(map);

  const markers = [];
  for (const issue of issues) {
    const marker = L.marker([issue.latitude, issue.longitude]).addTo(map);
    marker.bindPopup(`
            <strong>${escapeMapHtml(issue.title)}</strong><br>
            <span>${escapeMapHtml(issue.status)}</span><br>
            <small>${escapeMapHtml(issue.location)}</small><br>
            <a href="/issues">Shiko raportimet</a>
        `);
    markers.push(marker);
  }

  if (markers.length > 1) {
    const bounds = L.featureGroup(markers).getBounds().pad(0.15);
    map.fitBounds(bounds.intersects(pristinaBounds) ? bounds : pristinaBounds);
  }
};

let locationPickerMap;
let locationPickerMarker;
let locationPickerSelection;
let pristinaMapBounds;

function getPristinaMapBounds() {
  return (pristinaMapBounds ??= L.latLngBounds([42.58, 21.08], [42.75, 21.3]));
}

window.initializeLocationPicker = function (elementId, latitude, longitude) {
  const mapElement = document.getElementById(elementId);
  if (!mapElement || !window.L) {
    return;
  }

  locationPickerMap = L.map(mapElement, {
    maxBounds: getPristinaMapBounds(),
    maxBoundsViscosity: 1,
    minZoom: 11,
  }).setView(
    [latitude ?? 42.6629, longitude ?? 21.1655],
    latitude && longitude ? 16 : 13,
  );

  L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
    attribution: "&copy; OpenStreetMap contributors",
    maxZoom: 19,
  }).addTo(locationPickerMap);

  if (latitude && longitude) {
    setLocationPickerSelection(
      latitude,
      longitude,
      `Lat: ${latitude.toFixed(6)}, Lng: ${longitude.toFixed(6)}`,
    );
  }

  locationPickerMap.on("click", (event) => {
    setLocationPickerSelection(event.latlng.lat, event.latlng.lng);
  });
};

window.searchLocationPicker = async function (query) {
  const parameters = new URLSearchParams({
    q: `${query}, Prishtinë, Kosovo`,
    format: "jsonv2",
    limit: "1",
    bounded: "1",
    viewbox: "21.08,42.75,21.30,42.58",
  });
  const response = await fetch(
    `https://nominatim.openstreetmap.org/search?${parameters}`,
  );
  const results = await response.json();
  if (!results.length) {
    return null;
  }

  const result = results[0];
  return await setLocationPickerSelection(
    Number(result.lat),
    Number(result.lon),
    result.display_name,
  );
};

window.locateLocationPicker = function () {
  return new Promise((resolve, reject) => {
    if (!navigator.geolocation) {
      reject("Geolocation is not supported by this browser.");
      return;
    }

    navigator.geolocation.getCurrentPosition(async (position) => {
      const { latitude, longitude } = position.coords;
      if (!getPristinaMapBounds().contains([latitude, longitude])) {
        resolve(null);
        return;
      }

      resolve(await setLocationPickerSelection(latitude, longitude));
    }, reject);
  });
};

window.getLocationPickerSelection = function () {
  return locationPickerSelection;
};

window.disposeLocationPicker = function () {
  if (locationPickerMap) {
    locationPickerMap.remove();
  }
  locationPickerMap = null;
  locationPickerMarker = null;
  locationPickerSelection = null;
};

async function setLocationPickerSelection(latitude, longitude, address) {
  if (!getPristinaMapBounds().contains([latitude, longitude])) {
    return null;
  }

  if (!locationPickerMarker) {
    locationPickerMarker = L.marker([latitude, longitude], {
      draggable: true,
    }).addTo(locationPickerMap);
    locationPickerMarker.on("dragend", (event) => {
      const position = event.target.getLatLng();
      setLocationPickerSelection(position.lat, position.lng);
    });
  } else {
    locationPickerMarker.setLatLng([latitude, longitude]);
  }

  locationPickerMap.setView(
    [latitude, longitude],
    Math.max(locationPickerMap.getZoom(), 16),
  );
  const location = address ?? (await reverseGeocode(latitude, longitude));
  locationPickerSelection = {
    location:
      location ?? `Lat: ${latitude.toFixed(6)}, Lng: ${longitude.toFixed(6)}`,
    latitude,
    longitude,
  };
  return locationPickerSelection;
}

async function reverseGeocode(latitude, longitude) {
  try {
    const parameters = new URLSearchParams({
      lat: latitude,
      lon: longitude,
      format: "jsonv2",
      zoom: "18",
      addressdetails: "1",
    });
    const response = await fetch(
      `https://nominatim.openstreetmap.org/reverse?${parameters}`,
    );
    const result = await response.json();
    return result.display_name;
  } catch {
    return null;
  }
}

function escapeMapHtml(value) {
  const element = document.createElement("div");
  element.textContent = value ?? "";
  return element.innerHTML;
}

// window.getCurrentLocation = function () {
//     alert("JavaScript po funksionon!");
//     return Promise.resolve("JavaScript works");
// };
