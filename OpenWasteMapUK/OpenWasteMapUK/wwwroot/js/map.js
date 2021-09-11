// ==========
// GLOBALS
// ==========
var councilLayer;
var HWRCLayer = new L.LayerGroup();
var map;

// ==========
// EVENTS
// ==========

$(document).ready(function () {
    initMap();
    initDropdown();
    initHWRCs();
});

$("#searchSubmit").click(function () {
    search();
});

// ==========
// INITS
// ==========

function initMap() {
    map = L.map("map").setView([55.770394, -3.339844], 6);
    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
        {
            attribution: '&copy; <a href=\"https://openstreetmap.org\">OpenStreetMap</a> Contributors',
            maxZoom: 18,
            minZoom: 5
        }).addTo(map);
}

function initDropdown() {
    $("#searchWaste").select2({
        theme: "bootstrap4",
        placeholder: "Select a waste type",
        allowClear: true
    });
}

function initHWRCs() {
    $.ajax({
        url: "/api/GetFeatures",
        success: function (data) {
            for (var i = 0; i < data.length; i++) {
                if (data[i].tags != undefined) {
                    var marker;

                    if (data[i].type === "way") {
                        var nodesPoints = new Array();
                        for (var node in data[i].nodes) {
                            nodesPoints.push(findNodeLatLng(data[i].nodes[node], data));
                        }
                        nodesPoints.pop();
                        var buildingPolygon = new L.Polygon(nodesPoints).addTo(map);
                        marker = L.marker(buildingPolygon.getBounds().getCenter()).addTo(map);

                    } else {
                        marker = L.marker([data[i].lat, data[i].lon]);
                    }

                    let centreProps = data[i].tags;

                    var acceptedMaterials = Object.keys(centreProps).filter(function (k) {
                        return k.indexOf("recycling:") === 0 && (centreProps[k] === "yes" || centreProps[k].toLowerCase() === "no");
                    }).reduce(function (newData, k) {
                        newData[k] = centreProps[k];
                        return newData;
                    }, {});


                    var idPrefix = centreProps.name.replace(/^[^a-z]+|[^\w:.-]+/gi, "");

                    var popupData;



                    // Centre title
                    popupData = `<h6>${centreProps.name}</h6>`;

                    // Centre address
                    var addressContent = [];

                    if (centreProps["addr:housename"]) {
                        addressContent.push(centreProps["addr:housename"]);
                    }

                    if (centreProps["addr:housenumber"]) {
                        addressContent.push(centreProps["addr:housenumber"]);
                    }

                    if (centreProps["addr:street"]) {
                        addressContent.push(centreProps["addr:street"]);
                    }

                    if (centreProps["addr:city"]) {
                        addressContent.push(centreProps["addr:city"]);
                    }

                    if (centreProps["addr:postcode"]) {
                        addressContent.push(centreProps["addr:postcode"]);
                    }

                    popupData += "<em class=\"text-muted\">";
                    popupData += addressContent.join(", ");
                    popupData += "</em>";

                    // OSM link
                    var type = data[i].type;
                    var id = data[i].id;
                    var osmLink = `https://openstreetmap.org/${type}/${id}`;
                    popupData += `<div><a href="${osmLink}" target="_blank"><i class="fas fa-globe"></i> OpenStreetMap</a></div>`;

                    if (centreProps.website) {
                        popupData += `<div><a href="${centreProps.website}" target="_blank"><i class="fas fa-link"></i> Website</a></div>`;
                    }

                    // Accordion wrapper begin
                    popupData += `<div class="accordion mb-1" id="` + idPrefix + `Accordion">`;

                    // Accepted materials section
                    popupData += `<div class="card">`;
                    popupData += `<div class="card-header" id="` + idPrefix + `AcceptedMaterials">`;
                    popupData += `<h2 class="mb-0"><button class="btn btn-link btn-block text-left collapsed" type="button" data-toggle="collapse" data-target="#` + idPrefix + `AcceptedMaterialsMain" aria-expanded="true">Materials</button></h2></div>`;
                    popupData += `<div id="` + idPrefix + `AcceptedMaterialsMain" class="collapse" data-parent="#` + idPrefix + `Accordion"><div class="card-body">`;
                    popupData += "<ul class=\"fa-ul\">";
                    for (const [key, value] of Object.entries(acceptedMaterials)) {

                        if (!tagMap.hasOwnProperty(key)) {
                            console.warn("Missing tag for " + key);
                        } else {
                            switch (value.toLowerCase()) {
                                case "yes":
                                    popupData += "<li><span class=\"fa-li\"><i class=\"fas fa-check-square\" style=\"color:green\"></i></span>" + tagMap[key] + "</li>";
                                    break;
                                case "no":
                                    popupData += "<li><span class=\"fa-li\"><i class=\"fas fa-times-square\" style=\"color:red\"></i></span>" + tagMap[key] + "</li>";
                                    break;
                                default:
                                    console.warn("Unknown key/value pair supplied - " + key + " : " + value);
                                    break;
                            }
                        }


                    }
                    popupData += "</ul>";
                    popupData += `</div></div></div>`;

                    // Opening hours section
                    popupData += `<div class="card">`;
                    popupData += `<div class="card-header" id="` + idPrefix + `OpeningHours">`;
                    popupData += `<h2 class="mb-0"><button class="btn btn-link btn-block text-left collapsed" type="button" data-toggle="collapse" data-target="#` + idPrefix + `OpeningHoursMain" aria-expanded="true">Opening hours</button></h2></div>`;
                    popupData += `<div id="` + idPrefix + `OpeningHoursMain" class="collapse" data-parent="#` + idPrefix + `Accordion"><div class="card-body">`;

                    if (!!(centreProps["opening_hours"])) {

                        try {
                            var oh = new opening_hours(centreProps["opening_hours"]);
                            popupData += "<table style=\"width:100%\" class=\"open-time-table\">";
                            var currentDate = moment();
                            var todayNo = currentDate.day();
                            var weekStart = currentDate.clone().startOf("isoWeek").toDate();
                            var weekEnd = currentDate.clone().endOf("isoWeek").toDate();
                            var weekDays = getCurrentWeek();

                            var openTimes = oh.getOpenIntervals(weekStart, weekEnd);

                            for (var day in weekDays) {

                                let currentDayNo = weekDays[day].day();

                                if (todayNo === currentDayNo) {
                                    popupData += "<tr class=\"font-weight-bold\">";
                                } else {
                                    popupData += "<tr>";
                                }

                                popupData += "<td>" + weekDays[day].format("dddd") + "</td>";

                                var dayData = findOpenTimeMatch(openTimes, weekDays[day]);


                                if (dayData.length > 0) {
                                    popupData += "<td style=\"text-align:right\">";
                                    for (var time in dayData) {
                                        popupData += dayData[time][0].format("HH:mm") + " - " + dayData[time][1].format("HH:mm");
                                        popupData += "</br>";
                                    }
                                    popupData += "</td>";
                                } else {
                                    popupData += "<td style=\"text-align:right\">Closed</td>";
                                }


                                popupData += "</tr>";
                            }
                            popupData += "</table>";
                        } catch (ex) {

                            console.error("EXCEPTION CAUGHT: " + centreProps["opening_hours"] + "\n" + ex);
                        }


                    } else {
                        popupData += "<em>Opening hours not supplied</em>";
                    }


                    popupData += `</div></div></div>`;

                    // Accordion wrapper end
                    popupData += "</div>";


                    // TODO: Wikidata image
                    if (centreProps["image"]) {
                        popupData += "<img src=\"" + centreProps["image"] + "\" class=\"w-100\">";
                    }

                    var coords = marker.getLatLng();
                    popupData += `<button class="btn btn-sm text-white btn-primary suggest-changes-btn" data-toggle="modal" data-target="#suggestChangeModal" data-hwrc="${centreProps.name}" data-lat="${coords.lat}" data-lng="${coords.lng}" onclick="setSuggestedChanges(this)">Suggest a change</button>`;


                    marker.bindPopup(popupData, { maxWidth: 300, minWidth: 200 });
                    marker.properties = centreProps;
                    marker.addTo(HWRCLayer);
                }
            }
            HWRCLayer.addTo(map);
        }
    }).fail(function (data) { console.error(data); });
}

// ==========
// FUNCTIONS
// ==========

function search() {
    const postcode = $("#searchPostcode").val();
    const waste = $("#searchWaste").val();
    const searchError = $("#searchError");
    const searchResults = $("#searchResults");
    searchError.html("");
    searchResults.html("");
    if (councilLayer != null) {
        councilLayer.remove();
    }

    $.ajax({
        url: "/api/search",
        data: {
            postcode: postcode,
            waste: waste
        },
        success: function (data) {
            $.ajax({
                url: `/assets/geo/local-authorities/${data.councilCode}.geojson`
            }).done(function (councilData) {
                councilLayer = new L.GeoJSON(councilData,
                    {
                        style: { color: "green", opacity: 0.5 }
                    }).addTo(map);

                map.flyToBounds(councilLayer.getBounds(),
                    {
                        animate: true
                    });
            });

            var resultText = `<p>Your local authority area is <strong>${data.councilArea}</strong></p>`;

            if (data.wasteOSMTag == null) {
                resultText += `<p class="text-danger">Sorry. We don't recognise this waste type at the moment. If you know any recycling centres that take this waste type. Please <a href="#">tell us here</a>.</p>`;
                searchResults.html(resultText);
                return;
            }

            let hwrcMatchCount = 0;
            let listText = '<ul class="list-group list-group-flush">';

            debugger;

            HWRCLayer.eachLayer(function (layer) {
                // TODO: Create a function for this
                var icon = L.icon({
                    iconUrl: "",
                    iconSize: [25, 41],
                    iconAnchor: [12, 41],
                    popupAnchor: [1, -34],
                    shadowSize: [41, 41]
                });
                icon.options.iconUrl = "/lib/leaflet/images/marker-icon-blue.png";
                if (layer.properties.hasOwnProperty(data.wasteOSMTag) && layer.properties[data.wasteOSMTag] === "yes") {
                    if ((layer.properties.hasOwnProperty("owner") && layer.properties.owner.includes(data.councilArea)) || (layer.properties.hasOwnProperty("gssCode") && layer.properties.gssCode.includes(data.councilCode))) {
                        icon.options.iconUrl = "/lib/leaflet/images/marker-icon-green.png";
                        listText += `<li class="list-group-item">${layer.properties.name}</li>`;
                        hwrcMatchCount++;
                    }
                }

                layer.setIcon(icon);
            });
            listText += "</ul>";

            let sanitizedWasteType = sanitize(waste.toLowerCase());

            if (hwrcMatchCount > 0) {
                resultText += `<p>You can dispose of ${sanitizedWasteType} at the following locations:</p>`;
                resultText += listText;
            } else {
                resultText += `<p>Sorry, we can&apos;t find any recycling centres in your area that accept ${sanitizedWasteType}.</p>`;
            }

            searchResults.html(resultText);
        }
    }).fail(function (data) { searchError.html(data.responseText); });
};

function setSuggestedChanges(e) {
    $("#suggestChangeModalLabel").text(`Suggest a change for ${e.dataset.hwrc}`);
    $("#suggestHWRC").val(e.dataset.hwrc);
    $("#suggestLat").val(e.dataset.lat);
    $("#suggestLng").val(e.dataset.lng);
}

function submitChanges() {
    var hwrcName = $("#suggestHWRC").val();
    var hwrcText = $("#suggestedChangeText").val();
    var lat = $("#suggestLat").val();
    var lng = $("#suggestLng").val();
    $.ajax({
        url: "/api/SuggestChange",
        data: {
            hwrcName,
            hwrcText,
            lat,
            lng
        }
    }
    );
}

function findNodeLatLng(nodeName, nodes) {
    for (var node in nodes) {
        if (nodes[node].id === nodeName) {
            return [nodes[node].lat, nodes[node].lon];
        }
    }
}

function getCurrentWeek() {
    var currentDate = moment();

    var weekStart = currentDate.clone().startOf("isoWeek");

    var days = [];

    for (var i = 0; i <= 6; i++) {
        days.push(moment(weekStart).add(i, "days"));
    }

    return days;
}

function findOpenTimeMatch(openTimes, date) {
    var openingData = [];
    for (var i in openTimes) {
        var startTime = new moment(openTimes[i][0]);
        var endTime = new moment(openTimes[i][1]);
        if (date.startOf("day").isSame(startTime.startOf("day"))) {
            openingData.push([new moment(openTimes[i][0]), endTime]);
        }
    }
    return openingData;
}

function sanitize(string) {
    const map = {
        '&': "&amp;",
        '<': "&lt;",
        '>': "&gt;",
        '"': "&quot;",
        "'": "&#x27;",
        "/": "&#x2F;",
        "`": "&grave;"
    };
    const reg = /[&<>"'/]/ig;
    return string.replace(reg, (match) => (map[match]));
}