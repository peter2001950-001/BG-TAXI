
function worker(data) {
    var xmlHttp = new XMLHttpRequest();

    xmlHttp.onreadystatechange = function () {
        if (xmlHttp.readyState == 4 && xmlHttp.status == 200) {
            var responseObject = JSON.parse(xmlHttp.responseText);
            if (responseObject != undefined) {
                var requestId = responseObject.requestId;
                postMessage(responseObject.status=="OK"? "The request has been taken! Please wait for confirmation ..." : "An error occured while sending the request!");
                getStatus(responseObject.requestId, data.hash)

            }
        }
    }
    xmlHttp.open("POST", "http://peter200195-001-site1.btempurl.com/request/createNewRequest?lon=" + data.longitude + "&lat=" + data.latitude + "&basicAuth=" + data.hash, false);
    xmlHttp.send(null);
    // $.ajax({
    //     url: "http://peter200195-001-site1.btempurl.com/request/createNewRequest?lon="+ positionCou.longitude+ "&lat=" + positionCou.latitude + "&basicAuth=" + hash,
    //     type: "POST",
    //     dataType: "json",
    //     contentType: "application/json",
    //     success: function (status) {
    //             alert(status.status);
    //         if (status.status == "OK") {
    //             alert(status.status);
    //             postMessage(status.message);
    //             var requestId = status.requestId;
    //             var timer = setInterval(function () {
    //                 $.ajax({
    //                     url: "http://peter200195-001-site1.btempurl.com/request/requestStatus?requestID=" + requestId + "&basicAuth=" + hash,
    //                     type: "POST",
    //                     dataType: "json",
    //                     contentType: "application/json",
    //                     success: function (status) {
    //                         if (status.status == "TAKEN") {
    //                             clearInterval(timer);
    //                             postMessage("The request is TAKEN");
    //                         }else{
    //                             postMessage("Your request has been send! Please wait for confirmation ...");
    //                         }
    //                     },
    //                     error: function () {
    //                         postMessage("Error");
    //                         alert("error comunicating with serveer");
    //                     }
    //                 });
    //             }, 10000);
    //         }
    //     },
    //     error: function () {
    //         $("#messageBox").html("Error");
    //         alert("error");
    //     }

    // });
}
function getStatus(requestId, hash){
    var timer = setInterval(function () {
                    var xmlHttpStatus = new XMLHttpRequest();
                    xmlHttpStatus.onreadystatechange = function () {
                        if (xmlHttpStatus.readyState == 4 && xmlHttpStatus.status == 200) {

                            var responseObjectStatus = JSON.parse(xmlHttpStatus.responseText);
                            if (responseObjectStatus != null) {

                                if (responseObjectStatus.status == "TAKEN") {
                                    
                                    clearInterval(timer);
                                    postMessage("The request is TAKEN");
                                } else {
                                    postMessage("Your request has been send! Please wait for confirmation ...");
                                }
                            }
                        }
                    }
                    xmlHttpStatus.open("POST", "http://peter200195-001-site1.btempurl.com/request/requestStatus?requestID=" + requestId + "&basicAuth=" + hash, false);
    xmlHttpStatus.send(null);
                }, 10000);
}
onmessage = function (data) {
    worker(data.data);
}