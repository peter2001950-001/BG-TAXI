'use strict';

app.mainView = kendo.observable({
    onShow: function () {
        localStorage.setItem("answerOpened", "false");
        localStorage.setItem("requestOpened", "false");
        if(getFromLocalStorage("PullStarted") != "true"){
        pullRequests.startPull();

        }
    },
    afterShow: function () { }
});
app.localization.registerView('mainView');


function logout(){
    pullRequests.stopPull();
    $.ajax({
                    url: "http://bgtaxi.net/account/logoutExternal?accessToken=" + getFromLocalStorage("accessToken"),
                    type: "POST",
                    dataType: "json",
                    contentType: "application/json",
                    success: function (status) {
                        localStorage.setItem("accessToken", status.accessToken);
                        if (status.status == "OK") {
                            localStorage.removeItem("user");
                            localStorage.removeItem("userShortInfo");
                            localStorage.removeItem("userFirstName");
                            localStorage.removeItem("userLastName");
                            localStorage.removeItem("carIN"); 
                             app.mobileApp.navigate('components/home/view.html');
                        }
                         localStorage.setItem("ActiveRequest", "false");
                    },
                    error: function () {
                        alertStatus("Проблем при свързването със сървъра", "#167cf9");
                        localStorage.setItem("ActiveRequest", "false");
                        
                    }
                });
}

var pullRequests = (function (){
    var timer;
    var geotimer;
    var errors = 0;
    var positionCou;
    var gpsError = true;
    function setInformation() {
    var userTb = document.getElementsByClassName("header-user-text")[0];
    userTb.innerHTML = localStorage.getItem("userShortInfo");
    switch (localStorage.getItem("status")) {
        case "free":
            alertStatus("СВОБОДЕН", "#7cf477", "#000000");
            break;
        case "busy":
            alertStatus("ЗАЕТ", "#ff283e");
            if(localStorage.getItem("requestOpened") == "false"){
            localStorage.setItem("requestOpened", "true");
            app.mobileApp.navigate('components/currentRequestView/view.html');
            }
            break;
        case "absent":
            alertStatus("ОТСЪСТВАЩ", "#ffa500" , "#000000");
            document.getElementById("absentFreeBtn").innerHTML = "ВЪРНАХ СЕ";
            break;
        default:
            localStorage.setItem("status", "free");
            alertStatus("СВОБОДЕН", "#7cf477", "#000000");
            break;
    }
}

    function startTracking(){
        var done = true;
        geotimer = setInterval(function(){
            if(done){
                done = false;
            navigator.geolocation.getCurrentPosition(geoSuccess, geoError,  {enableHighAccuracy: true, timeout: 30000 });
            }
            function geoSuccess(position) {
                positionCou = position.coords;

                gpsError = false;
                done = true;
                console.log("GPS OK");
            }
            function geoError() {
                gpsError = true;
                alertStatus("Търсене на GPS сигнал ."+ errors+ ".", "#cccccc");

                console.log("GPS ERROR");
                errors++;
                done = true;
            }
        }, 1000);
    }
    function stopTracking(){
        clearInterval(geotimer);
    }

    function startPull(){
        startTracking();
    localStorage.setItem("ActiveRequest", "false");
    localStorage.setItem("PullStarted", "true");
    search();
    timer = setInterval(search, 3000);
    function search() {
        if (localStorage.getItem("ActiveRequest") == "false" && !gpsError) {
            localStorage.setItem("ActiveRequest", "true");
            
                $.ajax({
                    url: "http://bgtaxi.net/request/pull?lon=" + positionCou.longitude + "&lat=" + positionCou.latitude +  "&onAddress=" +  getFromLocalStorage("onAddress") + "&absent=" + getFromLocalStorage("absent")+ "&free=" + getFromLocalStorage("free") + "&accessToken=" + getFromLocalStorage("accessToken"),
                    type: "POST",
                    dataType: "json",
                    contentType: "application/json",
                    success: function (status) {
                        localStorage.setItem("accessToken", status.accessToken);
                        if (status.status == "OK") {
                            setInformation();
                            saveInLocalStorage("absent", "false");
                            saveInLocalStorage("free", "false");
                            if(status.request != undefined){
                                saveInLocalStorage("currentRequestStartAddress", status.request.startAddress);
                                saveInLocalStorage("currentRequestFinishAddress", status.request.finishAddress);
                                saveInLocalStorage("currentRequestDistance", status.request.distance);
                                saveInLocalStorage("currentRequestTime", status.request.time);
                                  saveInLocalStorage("currentRequestId", status.request.id);
                                  if(localStorage.getItem("answerOpened") == "false"){
                                app.mobileApp.navigate('components/requestsView/view.html');
                                  }
                            }
                            if(status.onAddress == true){
                                localStorage.setItem("onAddress", "false");
                                onAddressChange();
                            }

                        }else if (status.status == "NO PERMISSIION") {
                            alert("Грешка при удостоверяването на самоличността");
                            localStorage.removeItem("basicAuth");
                            app.mobileApp.navigate('components/home/view.html');
                        }else if (status.status == "INVALID ACCESSTOKEN") {
                            alertStatus("Грешка при самоличността","#167cf9");
                            console.log("ACCESSTOKEN");
                        }
                         localStorage.setItem("ActiveRequest", "false");
                    },
                    error: function () {
                        alertStatus("Проблем при свързването със сървъра", "#167cf9");
                        localStorage.setItem("ActiveRequest", "false");
                        
                    }
                });
        }
    }

    }
    function stopPull(){

    localStorage.setItem("PullStarted", "false");
        clearInterval(timer);
        stopTracking();
    }

    return{
        startPull: startPull,
        stopPull: stopPull
    }
}());


        function absentFree(){
        if(localStorage.getItem("status") == "free"){
        localStorage.setItem("absent", "true");
        localStorage.setItem("free", "false");
        localStorage.setItem("status", "absent");
        document.getElementById("absentFreeBtn").innerHTML = "ВЪРНАХ СЕ";

        }else if(localStorage.getItem("status") == "absent"){ 
        localStorage.setItem("absent", "false");
        localStorage.setItem("free", "true");
        localStorage.setItem("status", "free");
        document.getElementById("absentFreeBtn").innerHTML = "ОТСЪСТВАМ";
        }
    }
function alertStatus(message, color , textColor = "#ffffff") {
    var text = document.getElementsByClassName("header-status-text")[0];
    text.innerHTML = message;
    text.style.color = textColor;
    text.style.fontSize = "";
    text.style.padding = "6px";
    var node = text.parentNode;
    node.style.backgroundColor = color;
    var paddings = 12;
     while( $('.header-status-text').height() +paddings > $('.header-status').height() ) {
         $('.header-status-text').css('padding', "0px" );
        $('.header-status-text').css('font-size', (parseInt($('.header-status-text').css('font-size')) - 1) + "px" );
        paddings = 0;
    }
}
function goToMap(){
       app.mobileApp.navigate('components/requestsView/view.html');
}
// START_CUSTOM_CODE_mainView
// Add custom code here. For more information about custom code, see http://docs.telerik.com/platform/screenbuilder/troubleshooting/how-to-keep-custom-code-changes

// END_CUSTOM_CODE_mainView