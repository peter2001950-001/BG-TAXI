'use strict';

app.currentRequestView = kendo.observable({
    onShow: function() {
        $("#accepted-address").html("Адрес: " + getFromLocalStorage("selectedRequestAddress"));
        updating();
    },
    afterShow: function() {}
});
var onAddress = false;

function updating(){
    timer = setInterval(function () {
       
        navigator.geolocation.getCurrentPosition(geoSuccess, geoError, { timeout: 5000 });
        function geoSuccess(position) {
            var positionCou = position.coords;

            var requestId = getFromLocalStorage("selectedRequestId");
            saveInLocalStorage("currentLon", positionCou.longitude);
            saveInLocalStorage("currentLat", positionCou.latitude);
            $.ajax({
                url: "http://bgtaxi.net/request/updateStatus?lon=" + positionCou.longitude + "&lat=" + positionCou.latitude + "&basicAuth=" + getFromLocalStorage("basicAuth") + "&requestId=" + requestId +  "&onAddress=" + onAddress,
                type: "POST",
                dataType: "json",
                contentType: "application/json",
                success: function (status) {
                    if (status.status = "OK") {
                        var box = document.getElementById("clientStatus");
                        if(status.clientInformed){
                            box.innerHTML = "Клиентът е ИЗВЕСТЕН"
                            box.style.color = "#14f72f";
                            if(onAddress){
                               clearInterval(timer);
                            }
                        }else{
                             box.innerHTML = "Клиентът НЕ е ИЗВЕСТЕН"
                              box.style.color = "#f71414";
                        }
                        hideMessage();
                    }else if(status.status == "NO PERMISSIION"){
                        alert("Грешка при удостоверяването на самоличността");
                        removeFromLocalStorage("basicAuth");
                    app.mobileApp.navigate('components/home/view.html');
                    }
                },
                error: function () {
                    alertMessage("Проблем при свързването със сървъра", "Грешка!", "danger");
                }
            });
        }

    }, 1500);
    function geoError() {
        alertMessage("Търсене на GPS сигнал ...", "Грешка", "danger");
    }
}
function onAddressFunc(){
    if(!onAddress){
    onAddress = true;
   var button=  document.getElementsByClassName("btn")[0];
   button.innerHTML = "ВРЪЗКА С КЛИЕНТ";
   button.className = "btn btn-success";
    }else{
    onAddress = false;
   var button=  document.getElementsByClassName("btn")[0];
   button.innerHTML = "НА АДРЕСА";
   button.className = "btn btn-primary";
        $.ajax({
                url: "http://bgtaxi.net/request/finishRequest?requestId=" + getFromLocalStorage("selectedRequestId") + "&basicAuth=" + getFromLocalStorage("basicAuth"),
                type: "POST",
                dataType: "json",
                contentType: "application/json",
                success: function (status) {
                    if (status.status = "OK") {

                        clearInterval(timer);
                        removeFromLocalStorage("selectedRequestId");
                        removeFromLocalStorage("currentLon");
                        removeFromLocalStorage("currentLat");
                        removeFromLocalStorage("selectedRequestAddress");
                    app.mobileApp.navigate('components/mainView/view.html');
                    }else if(status.status == "ERR"){
                        alert("Грешка при удостоверяването на самоличността");
                        removeFromLocalStorage("basicAuth");
                    app.mobileApp.navigate('components/home/view.html');
                    }
                },
                error: function () {
                    alertMessage("Проблем при свързването със сървъра", "Грешка!", "danger");
                }
            });
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
    element.className = "alert alert-" + typeOf;
    element.innerHTML = "<strong>" + strong + "</strong> " + message;

    document.getElementById('loading').style.visibility = "hidden";
}
function hideMessage(){
    var element = document.getElementById("messageBox");
    element.style.visibility = "hidden";
}


// START_CUSTOM_CODE_currentRequestView
// Add custom code here. For more information about custom code, see http://docs.telerik.com/platform/screenbuilder/troubleshooting/how-to-keep-custom-code-changes

// END_CUSTOM_CODE_currentRequestView