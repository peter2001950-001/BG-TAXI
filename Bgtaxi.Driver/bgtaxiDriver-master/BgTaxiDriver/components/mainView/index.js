'use strict';

app.mainView = kendo.observable({
    onShow: function() {},
    afterShow: function() {}
});

function searchForRequests(){
     app.mobileApp.navigate('components/requestsView/view.html');
}

// START_CUSTOM_CODE_mainView
// Add custom code here. For more information about custom code, see http://docs.telerik.com/platform/screenbuilder/troubleshooting/how-to-keep-custom-code-changes

// END_CUSTOM_CODE_mainView