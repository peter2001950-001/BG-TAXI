
      var xmlHttp = new XMLHttpRequest();
    xmlHttp.open( "POST", "http://bgtaxi.net/Account/DeviceRegistration", false ); // false for synchronous request
    xmlHttp.send( null );
    var resp =  JSON.parse(xmlHttp.responseText);
    postMessage(resp.accessToken);

       