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

                resolve(
                    `Lat: ${latitude.toFixed(6)}, Lng: ${longitude.toFixed(6)}`
                );
            },
            function () {
                reject("Unable to get your location.");
            }
        );
    });
};

// window.getCurrentLocation = function () {
//     alert("JavaScript po funksionon!");
//     return Promise.resolve("JavaScript works");
// };