function DashboardViewModel() {
    var self = this;

    self.startingAddress = ko.observable();
    self.finishAddress = ko.observable();
    self.requestsData = ko.observable();
    var statusDesign = [
        { "code": "0", "message": "Изпращане на подходяща кола...", "type": "label-warning" },
        { "code": "1", "message": "Заявката приета", "type": "label-success" },
        { "code": "2", "message": "Заявката отхвърлена", "type": "label-danger" },
        { "code": "3", "message": "Търсене на подходяща кола", "type": "label-default" }];
    
    self.createRequest = function () {
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            url: "Dashboard/CreateRequest?startingAddress=" + this.startingAddress() + "&finishAddress=" + this.finishAddress(),
            success: function (data) {
                if (data.status == "OK") {
                    self.startingAddress("");
                    self.finishAddress("");
                } else if (data.status == "INVALID STARTING ADDRESS") {
                    alert("INVALID STARTING ADDRESS");
                }
            }
        });
    }
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
                    if (data.requests[i].requestStatus == 1) {
                        data.requests[i].info = "Време: " + data.requests[i].duraction + "; Кола №" + data.requests[i].carId;
                    } else {
                        data.requests[i].info = "";
                    }
                }

                self.requestsData(data);
            }
        });

    };

    setInterval(self.pull, 1000);
}
ko.applyBindings(new DashboardViewModel());
