'use strict';

app.requestsView = kendo.observable({
    onShow: function () {
        startSearching();
    },
    afterShow: function () { }
});
var timer;

function clickRow(element) {
    saveInLocalStorage("selectedRequestAddress",element.childNodes[1].innerHTML );
    saveInLocalStorage("selectedRequestId", element.childNodes[0].innerHTML);
    var longitude = getFromLocalStorage("currentLon");
    var latitude = getFromLocalStorage("currentLat");
    $.ajax({
        url: "http://bgtaxi.net/request/requestInfo?lon=" + longitude + "&lat=" + latitude + "&requestId=" + element.childNodes[0].innerHTML,
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        success: function (status) {
            if (status.status = "OK") {
                hideMessage();
                var uluru = { lat: status.lat, lng: status.lon };
                var map = new google.maps.Map(document.getElementById('modal-map'), {
                    zoom: 16,
                    center: uluru
                });
                var marker = new google.maps.Marker({
                    position: uluru,
                    map: map
                });
                $("#modal-body-header").html("Разстояние: " + status.distance + "  Време: " + status.duration);
            }
        },
        error: function () {
            alertMessage("Проблем при свързването със сървъра", "Грешка!", "danger");
        }
    });
    $("#myModalLabel").html("Заявка за " + element.childNodes[1].innerHTML);
    $('#myModal').modal('toggle');
    clearInterval(timer);
}

function modalClosed() {
    startSearching();
}
function acceptRequest() {
    var longitude = getFromLocalStorage("currentLon");
    var latitude = getFromLocalStorage("currentLat");
    $.ajax({
        url: "http://bgtaxi.net/request/takeRequest?basicAuth=" + getFromLocalStorage("basicAuth")+ "&requestID=" + getFromLocalStorage("selectedRequestId") + "&lon=" +longitude + "&lat="+latitude ,
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        success: function (status) {
            if (status.status = "OK") {
                hideMessage();
                $('#myModal').modal('hide');
                app.mobileApp.navigate('components/currentRequestView/view.html');
            }else if (status.status == "REMOVED"){
                alert("Заявката е вече приета от друг шофьор");
            }
        },
        error: function () {
            alertMessage("Проблем при свързването със сървъра", "Грешка!", "danger");
        }
    });
}

function startSearching() {
    search();
    timer = setInterval(search, 3000);
    function search() {
       
        navigator.geolocation.getCurrentPosition(geoSuccess, geoError, { timeout: 5000 });

        function geoSuccess(position) {
            var positionCou = position.coords;

            saveInLocalStorage("currentLon", positionCou.longitude);
            saveInLocalStorage("currentLat", positionCou.latitude);
            $.ajax({
                url: "http://bgtaxi.net/request/approRequest?lon=" + positionCou.longitude + "&lat=" + positionCou.latitude + "&basicAuth=" + getFromLocalStorage("basicAuth"),
                type: "POST",
                dataType: "json",
                contentType: "application/json",
                success: function (status) {
                    if (status.status = "OK") {
                        hideMessage();
                        AddToTable(status.requests);
                    }else if(status.status == "NO PERMISSIION"){
                        alert("Грешка при удостоверяването на съмоличността");
                        removeFromLocalStorage("basicAuth");
                    app.mobileApp.navigate('components/home/view.html');
                    }
                },
                error: function () {
                    alertMessage("Проблем при свързването със сървъра", "Грешка!", "danger");
                }
            });
        }
        function geoError() {
        alertMessage("Търсене на GPS сигнал ...", "Грешка", "danger");
    }
    }
    
}


function saveInLocalStorage(key, info) {
    if (localStorage) {
        localStorage.setItem(key, info);
    } else {
        app[key] = info;
    }
}
function getFromLocalStorage(key) {
    if (localStorage) {
        return localStorage.getItem(key);
    } else {
        return app[key];
    }
}
function removeFromLocalStorage(key) {
    if (localStorage) {
        localStorage.removeItem(key);
    } else {
        app[key] = null;
    }
}
function alertMessage(message, strong, typeOf) {
    var element = document.getElementById("messageBox");

    element.style.visibility = "visibile";
    element.className = "alert alert-" + typeOf;
    element.innerHTML = "<strong>" + strong + "</strong> " + message;

    document.getElementById('loading').style.visibility = "hidden";
}
function hideMessage(){
    var element = document.getElementById("messageBox");
    element.style.visibility = "hidden";
}

function AddToTable(data) {
    $("#tableBody").empty();
    for (var i in data) {
        $("#tableBody").append("<tr onclick = 'clickRow(this)'><td>" + data[i].id + "</td><td>" + data[i].address + "</td><td>" + data[i].distance + "</td><td>НЕ ПРИЕТА</td></tr>")
    }

}
// START_CUSTOM_CODE_requestsView
// Add custom code here. For more information about custom code, see http://docs.telerik.com/platform/screenbuilder/troubleshooting/how-to-keep-custom-code-changes

// END_CUSTOM_CODE_requestsView