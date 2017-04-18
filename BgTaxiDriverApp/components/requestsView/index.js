'use strict';

app.requestsView = kendo.observable({
    onShow: function() {
        console.log("requestsView shown");
     document.getElementById("RequestViewScreen").style.height = (document.body.offsetHeight -45) + "px";
     localStorage.setItem("answerOpened", "true");
     document.getElementsByClassName("choice-no")[0].innerHTML = "НЕ (15)";
     document.getElementById("start-address").innerHTML = getFromLocalStorage("currentRequestStartAddress");
      document.getElementById("finish-address").innerHTML = "ЗА: " + getFromLocalStorage("currentRequestFinishAddress");
     document.getElementById("moreInfo").innerHTML = "Разстояние: " + getFromLocalStorage("currentRequestDistance") +  " км.";
     counter();
    },
    afterShow: function() {}
});
app.localization.registerView('requestsView');

function answer(bool){
    if(bool){
            alertStatus("Приемане на заявката...", "#FFFFFF" , "#000000" );
    }else{
            alertStatus("Отказване на заявката...", "#FFFFFF" , "#000000" );
    }
    var waitingInteral = setInterval(function
(){
    console.log("waiting....");
    if(getFromLocalStorage("ActiveRequest") == "false"){
        clearInterval(waitingInteral);
        localStorage.setItem("ActiveRequest", "true");
        $.ajax({
                    url: "http://bgtaxi.net/request/RequestAnswer?requestID=" + getFromLocalStorage("currentRequestId") + "&answer=" + bool + "&accessToken=" + getFromLocalStorage("accessToken"),
                    type: "POST",
                    dataType: "json",
                    contentType: "application/json",
                    success: function (status) {
                        localStorage.setItem("accessToken", status.accessToken);
                         localStorage.setItem("ActiveRequest", "false");
                        if (status.status = "OK") {
                            if(bool){
                                 localStorage.setItem("status", "busy");
                                app.mobileApp.navigate('components/currentRequestView/view.html');
                            }else{
                                
                                 localStorage.setItem("status", "free");
                                app.mobileApp.navigate('components/mainView/view.html');
                            }
                            
                        } else if (status.status == "REMOVED") {
                            alert("Срокът за приемане на заявката е изтекъл!");
                            app.mobileApp.navigate('components/mainView/view.html');
                        } else if (satus.status == "ERR") {
                            alert("Заявата беше неуспешно приета!");
                            app.mobileApp.navigate('components/mainView/view.html');
                        }
                    },
                    error: function () {
                        alertStatus("Проблем при свързването със сървъра", "#167cf9");
                         localStorage.setItem("ActiveRequest", "false");
                        
                    }
                });
    }
}, 50);
    }

function counter(){
    var value = getFromLocalStorage("currentRequestTime");
    var element = document.getElementsByClassName("choice-no")[0];
    var counter = setInterval(function(){
        if(value == 0){
            clearInterval(counter);
             app.mobileApp.navigate('components/mainView/view.html');
        }
        element.innerHTML = "НЕ (" + value +")";
        value-=1;
    }, 1000)
}
// START_CUSTOM_CODE_requestsView""
// Add custom code here. For more information about custom code, see http://docs.telerik.com/platform/screenbuilder/troubleshooting/how-to-keep-custom-code-changes

// END_CUSTOM_CODE_requestsView