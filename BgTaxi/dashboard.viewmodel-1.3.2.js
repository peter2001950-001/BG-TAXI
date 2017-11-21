var cars = [];
var bangalore = { lat: 42.137392, lng: 24.741973 };
var zoom = 12;
var map = new google.maps.Map(document.getElementById('map'), {
    zoom: zoom,
    center: bangalore

});
var markers = [];
ko.bindingHandlers.executeOnEnter = {
    init: function (element, valueAccessor, allBindings, viewModel) {
        var callback = valueAccessor();
        $(element).keypress(function (event) {
            var keyCode = (event.which ? event.which : event.keyCode);
            if (keyCode === 13) {
                callback.call(viewModel);
                return false;
            }
            return true;
        });
    }
};
function DashboardViewModel() {
    var self = this;
    var freeIcon = { url: "/Content/images/dashboard/FreeIcon.png", scaledSize: new google.maps.Size(30, 44), labelOrigin: new google.maps.Point(15, 15) };
    var busyIcon = { url: "/Content/images/dashboard/BusyIcon.png", scaledSize: new google.maps.Size(30, 44), labelOrigin: new google.maps.Point(15, 15) };
    var absentIcon = { url: "/Content/images/dashboard/AbsentIcon.png", scaledSize: new google.maps.Size(30, 44), labelOrigin: new google.maps.Point(15, 15) };
    var offdutyIcon = { url: "/Content/images/dashboard/Offduty.png", scaledSize: new google.maps.Size(30, 44), labelOrigin: new google.maps.Point(15, 15) };
    var offlineIcon = { url: "/Content/images/dashboard/OfflineIcon.png", scaledSize: new google.maps.Size(30, 44), labelOrigin: new google.maps.Point(15, 15) };
    
    var freeChecked = false;
    var busyChecked = false;
    var absentChecked = false;
    var offlineChecked = false;
    var offdutyChecked = false;

    self.startingAddress = ko.observable();
    self.finishAddress = ko.observable();
    self.requestsData = ko.observable();
    self.suggectionsData = ko.observable();
    self.foundRequetsData = ko.observable();
    self.requestInfoData = ko.observable();

    self.freeCarSpan = ko.observable();
    self.busyCarSpan = ko.observable();
    self.absentCarSpan = ko.observable();
    self.offlineCarSpan = ko.observable();
    self.offdutyCarSpan = ko.observable();
    var statusDesign = [
        { "code": "0", "message": "Изпращане на подходяща кола...", "type": "label-warning" },
        { "code": "1", "message": "Заявката приета", "type": "label-success" },
        { "code": "2", "message": "Заявката отхвърлена", "type": "label-danger" },
        { "code": "3", "message": "Търсене на подходяща кола", "type": "label-default" },
        { "code": "4", "message": "Колата е на адреса", "type": "label-primary" },
        { "code": "5", "message": "Има връзка с клиент", "type": "label-primary" },
        { "code": "6", "message": "Няма клиент на адреса", "type": "label-primary" },
        { "code": "7", "message": "Приключена заявка", "type": "label-primary" }];
    
    self.timeRanges = ko.observableArray(["1", "2", "3", "6", "12", "24"]);
    self.selectedTimeRange = ko.observable();
    self.requestId = ko.observable();
    self.requestSearchedStartingAddress = ko.observable();
    self.searchRequests = function () {
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            url: "/Dashboard/SearchRequest?id=" + this.requestId()+"&startingAddress="+self.requestSearchedStartingAddress()+ "&period="+ self.selectedTimeRange(),
            success: function (data) {
                if (data.status === "OK") {
                    for (var i in data.requests) {
                        var status = statusDesign[data.requests[i].requestStatus];
                        data.requests[i].message = status.message;
                        data.requests[i].className = status.type;
                    }
                    self.foundRequetsData(data);
                }
            }
        });

    }
    self.autoSuggest = function() {

        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            url: "/Dashboard/AutoComplete?text=" + this.startingAddress(),
            success: function(data) {
                if (data.status === "OK") {
                    self.suggectionsData(data);
                }
            }
        });
    }
    
    self.localization = function (request) {
        console.log(request);
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            url: "/Dashboard/RequestLocation?id=" + request.id,
            success: function (data) {
                if (data.status === "OK") {
                    bangalore = { lat: data.lat, lng: data.lng };
                    console.log(bangalore);
                    zoom = 18;
                    map = new google.maps.Map(document.getElementById('map'), {
                        zoom: zoom,
                        center: bangalore
                    });
                }
            }
        });
    }
    self.showInformation = function (request) {
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            url: "/Dashboard/RequestInfo?id=" + request.id,
            success: function (data) {

               var parsedData=  JSON.parse(data);
                
               if (parsedData.status === "OK") {
                   
                   var status = statusDesign[parsedData.requestStatus];
                   parsedData.message = status.message;
                   parsedData.className = status.type;
                    if (parsedData.takenBy == undefined) {
                        parsedData.takenBy = "";
                    } 
                    if (parsedData.distance == undefined) {
                        parsedData.distance = "";
                    }
                    if (parsedData.duration == undefined) {
                        parsedData.duration = "";
                    }
                    
                    self.requestInfoData(parsedData);
                    $("#requestInfo").css("display", "");
                    $("#map-section").css("display", "none");
                    console.log(parsedData);
                }
            }
        });
    }
    self.select = function(item) {

        console.log(item);
        self.startingAddress(item.main_text);
    };

    self.createRequest = function() {
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            url: "/Dashboard/CreateRequest?startingAddress=" +
                this.startingAddress() +
                "&finishAddress=" +
                this.finishAddress(),
            success: function(data) {
                if (data.status === "OK") {
                    self.startingAddress("");
                    self.finishAddress("");
                } else if (data.status === "INVALID STARTING LOCATION") {
                    alert("Не намерен адрес");
                }
            }
        });
    };
    self.pull = function () {
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            url: "/Dashboard/Pull?free=" + freeChecked + "&busy=" + busyChecked + "&absent=" + absentChecked + "&offline="+ offlineChecked + "&offduty=" + offdutyChecked,
            success: function (data) {
                for (var i in data.requests) {
                    console.log(data.requests[i].requestStatus)
                    var status = statusDesign[data.requests[i].requestStatus];
                    data.requests[i].message = status.message;
                    data.requests[i].className = status.type;
                    if (data.requests[i].requestStatus === 1) {
                        data.requests[i].info = "Време: " + data.requests[i].duraction + "; Кола №" + data.requests[i].carId;
                    } else {
                        data.requests[i].info = "";
                    }
                }
                   
                
                cars = data.cars;

                function setMapOnAll(map) {
                    for (var i = 0; i < markers.length; i++) {
                        markers[i].setMap(map);
                    }
                }
                setMapOnAll(null);
                for (var i in cars) {
                    var icon;
                    switch (cars[i].carStatus) {
                        case 0:
                            icon = freeIcon;
                            console.log(icon);
                            break;
                        case 1:
                            icon = busyIcon;
                            break;
                        case 2:
                            icon = absentIcon;
                            break;
                        case 3:
                            icon = offdutyIcon;
                            break;
                        case 4:
                            icon = offlineIcon;
                            break;
                        default:

                    }
                    var marker = new google.maps.Marker({
                        position: { lat: cars[i].lat, lng: cars[i].lng },
                        label: { text: cars[i].id, fontSize: "10px" },
                        icon: icon,
                        map: map
                    });
                    markers.push(marker);
                }
                self.requestsData(data);

                self.freeCarSpan(data.freeStatusCount);
                self.busyCarSpan(data.busyStatusCount);
                self.absentCarSpan(data.absentStatusCount);
                self.offlineCarSpan(data.offlineStatusCount);
                self.offdutyCarSpan(data.offdutyStatusCount);
            }
        });

    };

  self.freeItemChecked = function () {
        if (freeChecked) {
            freeChecked = false;
            $(".free").removeClass("is-checked");
        } else {
            freeChecked = true;
            $(".free").addClass("is-checked");
        }
    }
    self.busyItemChecked = function () {
        if (busyChecked) {
            busyChecked = false;
            $(".busy").removeClass("is-checked");
        } else {
            busyChecked = true;
            $(".busy").addClass("is-checked");
        }
    }
    self.absentItemChecked = function () {
        if (absentChecked) {
            absentChecked = false;
            $(".absent").removeClass("is-checked");
        } else {
            absentChecked = true;
            $(".absent").addClass("is-checked");
        }
    }
    self.offlineItemChecked = function () {
        if (offlineChecked) {
            offlineChecked = false;
            $(".offline").removeClass("is-checked");
        } else {
            offlineChecked = true;
            $(".offline").addClass("is-checked");
        }
    }
    self.offdutyItemChecked = function () {
        if (offdutyChecked) {
            offdutyChecked = false;
            $(".offduty").removeClass("is-checked");
        } else {
            offdutyChecked = true;
            $(".offduty").addClass("is-checked");
        }
    }
    setInterval(self.pull, 1000);

  
}
ko.applyBindings(new DashboardViewModel());

google.maps.event.addDomListener(window, 'resize', function () {
    map.setCenter(bangalore);
});

function requestInfoClose(){

    $("#map-section").css("display", "");
    $("#requestInfo").css("display", "none");
    google.maps.event.trigger(map, 'resize');
}