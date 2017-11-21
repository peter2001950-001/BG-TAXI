var cars = [];
var bangalore = { lat: 42.137392, lng: 24.741973 };
var map = new google.maps.Map(document.getElementById('map'), {
    zoom: 12,
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
    self.startingAddress = ko.observable();
    self.finishAddress = ko.observable();
    self.requestsData = ko.observable();
    self.suggectionsData = ko.observable();
    var statusDesign = [
        { "code": "0", "message": "Изпращане на подходяща кола...", "type": "label-warning" },
        { "code": "1", "message": "Заявката приета", "type": "label-success" },
        { "code": "2", "message": "Заявката отхвърлена", "type": "label-danger" },
        { "code": "3", "message": "Търсене на подходяща кола", "type": "label-default" },
        { "code": "4", "message": "Колата е на адреса", "type": "label-primary" },
        { "code": "5", "message": "Има връзка с клиент", "type": "label-primary" },
        { "code": "6", "message": "Няма клиент на адреса", "type": "label-primary" },
        { "code": "7", "message": "Приключена заявка", "type": "label-primary" }];
    
    self.autoSuggest = function() {

        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            url: "Dashboard/AutoComplete?text=" + this.startingAddress(),
            success: function(data) {
                if (data.status === "OK") {
                    self.suggectionsData(data);
                }
            }
        });
    }
    self.select = function(item) {

        console.log(item);
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            url: "Dashboard/PlaceInfo?placeId=" +
                item.placeId,
            success: function (data) {
                if (data.status === "OK") {
                    map.setCenter({ lat: data.lat, lng: data.lon });
                    var marker = new google.maps.Marker({
                        position: { lat: data.lat, lng: data.lon },
                        map: map
                    });
                    markers.push(marker);
                    self.startingAddress(data.address);
                } else if (data.status === "INVALID STARTING LOCATION") {
                    alert("Не намерен адрес");
                }
            }
        });
        self.startingAddress(item.main_text);
    };

    self.createRequest = function() {
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            url: "Dashboard/CreateRequest?startingAddress=" +
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
            url: "Dashboard/Pull",
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
            }
        });

    };

    setInterval(self.pull, 1000);
}
ko.applyBindings(new DashboardViewModel());
