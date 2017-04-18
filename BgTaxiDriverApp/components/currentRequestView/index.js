'use strict';

app.currentRequestView = kendo.observable({
    onShow: function() {

        document.getElementById("start-address-text").innerHTML = getFromLocalStorage("currentRequestStartAddress");
        document.getElementById("finish-address-text").innerHTML = getFromLocalStorage("currentRequestFinishAddress");
        document.getElementById("CurrentRequestViewScreen").style.height = (document.body.offsetHeight -58) + "px";
        if(localStorage.getItem("IsOnAddress")== "true"){
             document.getElementById("onAddressBtn").innerHTML = "ВРЪЗКА С КЛИЕНТ";
        }
    },
    afterShow: function() {}
});
app.localization.registerView('currentRequestView');

function onAddress(){
    localStorage.setItem("onAddress", "true");
}
function onAddressChange(){
    document.getElementById("onAddressBtn").innerHTML = "ВРЪЗКА С КЛИЕНТ";
    localStorage.setItem("IsOnAddress", "true");
}

function finishRequest(){

var newWaitingInteral = setInterval(function
(){
    if(getFromLocalStorage("ActiveRequest") == "false"){
        clearInterval(newWaitingInteral);
        saveInLocalStorage("ActiveRequest", "true");
         $.ajax({
                    url: "http://bgtaxi.net/request/finishRequest?requestId=" + localStorage.getItem("currentRequestId") + "&accessToken=" + getFromLocalStorage("accessToken"),
                    type: "POST",
                    dataType: "json",
                    contentType: "application/json",
                    success: function (status) {
                         localStorage.setItem("accessToken", status.accessToken);
                        if (status.status == "OK") {
                              localStorage.removeItem("currentRequestStartAddress");
                               localStorage.removeItem("currentRequestFinishAddress");
                               localStorage.removeItem("currentRequestDistance");
                               localStorage.removeItem("currentRequestTime");
                               localStorage.removeItem("currentRequestId");
                               localStorage.removeItem("IsOnAddress");
                               localStorage.setItem("onAddress", "false");
                                localStorage.setItem("status", "free");

                             saveInLocalStorage("ActiveRequest", "false");
                            app.mobileApp.navigate('components/mainView/view.html');
                        } else {
                            init();
                        }
                    },
                    error: function () {
                        $("#messageBox").html("Error");
                         saveInLocalStorage("ActiveRequest", "false");
                        alert("error comunicating with serveer");
                    }
            });
    }
}, 50);
    
    
}
// START_CUSTOM_CODE_currentRequestView
// Add custom code here. For more information about custom code, see http://docs.telerik.com/platform/screenbuilder/troubleshooting/how-to-keep-custom-code-changes

// END_CUSTOM_CODE_currentRequestView