'use strict';

app.callTaxi = kendo.observable({
    onShow: function () {
        if (getFromLocalStorage("currentRequestId") != undefined) {
            var requestId = getFromLocalStorage("currentRequestId");
            var hash = getFromLocalStorage("basicAuth");
            alertMessage(getFromLocalStorage("currentRequestStatus"), "Изпратена!", "warning");
            document.getElementById("callTBtn").classList.add("disabled");
            var timer = setInterval(function () {
                        $.ajax({
                            url: "http://bgtaxi.net/request/requestStatus?requestID=" + requestId + "&basicAuth=" + hash,
                            type: "POST",
                            dataType: "json",
                            contentType: "application/json",
                            success: function (status) {
                                console.log(status.status);
                                switch(status.status){
                                    case "TAKEN":
                                    console.log("from taken");
                                    var Information = "Заявката беше приета успешно от " + status.companyName + "! Автомобилът е регистрационен номер: " + status.carRegNum + "</br> Приблизително време: " + status.distance;
                                    alertMessage(Information, "Приета!", "success");
                                    saveInLocalStorage("currentRequestStatus",Information);
                                    break;
                                    case "ON ADDRESS":
                                    console.log("from on address");
                                     alertMessage("Вашето такси пристигна на посочения от вас адрес. ", "", "info");
                                    $("#callTBtn").removeClass("disabled");
                                     clearInterval(timer);
                                    removeFromLocalStorage("currentRequestId");
                                    break;
                                    case "NO CHANGE":
                                    console.log("from no change");
                                    alertMessage(saveInLocalStorage(Information, "takenRequestInfo"), "Приета!", "success");
                                    break;
                                    case "NOT TAKEN":
                                    break;
                                    default:
                                    	alert(status.status);
                                    break;

                                }
                            },
                            error: function () {
                                alertMessage("Възникна грешка!", "Грешка", "danger");
                                $(".progress-bar").css("width", "0%");
                                $("#callTBtn").removeClass("disabled");
                                removeFromLocalStorage("currentRequestId");
                            }
                        });
                    }, 2000);
        }
    },
    afterShow: function () { },

});

function clicked() {
    document.getElementById("callTBtn").classList.add("disabled");
    $('#loading').css("visibility", "visible");
    $(".progress-bar").animate({
        width: "30%"
    }, 300);

    var rememberKey = 'basicAuth',
        auth = "";
    if (localStorage) {
        auth = localStorage.getItem(rememberKey);
    } else {
        auth = app[rememberKey];
    }
    
    navigator.geolocation.getCurrentPosition(geoSuccess, geoError, { timeout: 45000 });

    function geoSuccess(position) {

        $(".progress-bar").animate({
            width: "70%"
        }, 500);
        var positionCou = position.coords;
        $.ajax({
            url: "http://bgtaxi.net/request/createNewRequest?lon=" + positionCou.longitude + "&lat=" + positionCou.latitude + "&basicAuth=" + auth,
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            success: function (status) {
                $(".progress-bar").animate({
                    width: "100%"
                }, 500);
                if (status.status == "OK") {
                    var requestId = status.requestId;
                    var address = status.address;
                    var messageToShow = "Заявката беше изпратена за " + address + "! Моля, изчакайте нейното приемане ...";
                    alertMessage(messageToShow, "Изпратена!", "warning");
                    saveInLocalStorage("currentRequestStatus", messageToShow);
                    saveInLocalStorage("currentRequestId", requestId);
                    var timer = setInterval(function () {
                        console.log("shajda");
                        $.ajax({
                            url: "http://bgtaxi.net/request/requestStatus?requestID=" + requestId + "&basicAuth=" + auth,
                            type: "POST",
                            dataType: "json",
                            contentType: "application/json",
                            success: function (status) {
                                console.log(status.status);
                                switch(status.status){
                                    case "TAKEN":
                                    console.log("from taken");
                                    var Information = "Заявката беше приета успешно от " + status.companyName + "! Автомобилът е регистрационен номер: " + status.carRegNum + "</br> Приблизително време: " + status.distance;
                                    alertMessage(Information, "Приета!", "success");
                                    saveInLocalStorage("currentRequestStatus", Information);
                                    break;
                                    case "ON ADDRESS":
                                    console.log("from on address");
                                     alertMessage("Вашето такси пристигна на посочения от вас адрес. ", "", "info");
                                    $("#callTBtn").removeClass("disabled");
                                     clearInterval(timer);
                                    removeFromLocalStorage("currentRequestId");
                                    break;
                                    case "NO CHANGE":
                                    break;
                                    case "NOT TAKEN":
                                    break;
                                    default:
                                    	alert(status.status);
                                    break;

                                }
                            },
                            error: function () {
                                alertMessage("Възникна грешка!", "Грешка", "danger");
                                $(".progress-bar").css("width", "0%");
                                $("#callTBtn").removeClass("disabled");
                                removeFromLocalStorage("currentRequestId");
                            }
                        });
                    }, 2000);
                }
            },
            error: function () {
                alertMessage("Грешка при изпращането на заявката!", "Грешка", "danger");
                $("#callTBtn").removeClass("disabled");
                $(".progress-bar").css("width", "0%");
            }

        });

    }

    function geoError() {
        alertMessage("Не могат да се определят GPS кординати!", "Грешка", "danger");
        document.getElementById("callTBtn").className = "btn btn-warning btn-lg";
    }
}

function alertMessage(message, strong, typeOf) {
    var element = document.getElementById("messageBox");
    element.className = "alert alert-" + typeOf;
    element.innerHTML = "<strong>" + strong + "</strong> " + message;

    document.getElementById('loading').style.visibility = "hidden";
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



// START_CUSTOM_CODE_callTaxi
// Add custom code here. For more information about custom code, see http://docs.telerik.com/platform/screenbuilder/troubleshooting/how-to-keep-custom-code-changes

// END_CUSTOM_CODE_callTaxi
