'use strict';

app.companyCodeView = kendo.observable({
    onShow: function() {},
    afterShow: function() {}
});

// START_CUSTOM_CODE_companyCodeView
// Add custom code here. For more information about custom code, see http://docs.telerik.com/platform/screenbuilder/troubleshooting/how-to-keep-custom-code-changes

// END_CUSTOM_CODE_companyCodeView
(function(parent) {
    var companyCodeViewModel = kendo.observable({
            submit: function() {
                var addFormData = parent.get('addFormData');
                $.ajax({
        url: "http://bgtaxi.net/externalManage/addCompanyCode?basicAuth=" + localStorage.getItem("basicAuth") + "&companyCode=" + addFormData.companyCode,
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        success: function (status) {
            if (status.status = "OK") {
                app.mobileApp.navigate('components/home/view.html');
            }else if(status.status == "WRONG CODE"){
                companyCodeViewModel.set('errorMessage', 'Невалиден уникален номер.');
            }
        },
        error: function () {
            companyCodeViewModel.set('errorMessage', 'Грешка при свързването със сървъра.');
        }
    });
            },
            /// start add model functions
            /// end add model functions

            cancel: function() {
                   localStorage.removeItem("basicAuth");
                     app.mobileApp.navigate('components/home/view.html');
                }
                /// start add model properties
                /// end add model properties

        });

    /// start form functions
    /// end form functions

    parent.set('onShow', function _onShow() {
        var that = parent;
        that.set('addFormData', {
            companyCode: '',
            /// start add form data init
            /// end add form data init
        });
        /// start add form show
        /// end add form show
    });
    parent.set('companyCodeViewModel', companyCodeViewModel);
})(app.companyCodeView);

// START_CUSTOM_CODE_companyCodeViewModel
// Add custom code here. For more information about custom code, see http://docs.telerik.com/platform/screenbuilder/troubleshooting/how-to-keep-custom-code-changes

// END_CUSTOM_CODE_companyCodeViewModel