'use strict';

app.home = kendo.observable({
    onShow: function() {
        saveInLocalStorage("PullStarted", "false");
        localStorage.setItem("onAddress", "false");
        localStorage.setItem("absent", "false");
        localStorage.setItem("free", "false");
        if(getFromLocalStorage("accessToken") == "undefined" || getFromLocalStorage("accessToken") == null){
            startWorker();
        }else if(localStorage.getItem("user") != undefined || app["user"] != undefined){

            app.mobileApp.navigate('components/mainView/view.html');
       }
       
       },
    afterShow: function() {}
});

// START_CUSTOM_CODE_home
// Add custom code here. For more information about custom code, see http://docs.telerik.com/platform/screenbuilder/troubleshooting/how-to-keep-custom-code-changes

// END_CUSTOM_CODE_home
(function(parent) {
    var provider = app.data.bgTaxiDriver,
        mode = 'signin',
        rememberKey = 'bgTaxiDriver_Auth_authData_homeView',
        init = function(error, result) {
            $('.status').text('');

            if (error) {
                if (error.message) {
                    $('.status').text(error.message);
                }

                return false;
            }

            var activeView = mode === 'signin' ? '.signin-view' : '.signup-view',
                model = parent.homeModel;

            if (provider.setup && provider.setup.offlineStorage && !app.isOnline()) {
                $('.signin-view', 'signup-view').hide();
                $('.offline').show();
            } else {
                $('.offline').hide();

                if (mode === 'signin') {
                    $('.signup-view').hide();
                } else {
                    $('.signin-view').hide();
                }

                $(activeView).show();
            }

            if (model && model.set) {
                model.set('logout', null);
            }

        },
        homeModel = kendo.observable({
            displayName: '',
            email: '',
            password: '',
            errorMessage: '',
            validateData: function(data) {
                var model = homeModel;

                if (!data.email && !data.password) {
                    model.set('errorMessage', 'Missing credentials.');
                    return false;
                }

                if (!data.email) {
                    model.set('errorMessage', 'Missing username or email.');
                    return false;
                }

                if (!data.password) {
                    model.set('errorMessage', 'Missing password.');
                    return false;
                }

                return true;
            },
            signin: function() {
                var model = homeModel,
                    email = model.email.toLowerCase(),
                    password = model.password;

                if (!model.validateData(model)) {
                    return false;
                }
                while(getFromLocalStorage("accessToken") == undefined){
                    model.set('errorMessage', 'Моля, изчакайте...');
                }
                if(getFromLocalStorage("accessToken") == "NONE"){
                    model.set('errorMessage', 'Възникна грешка при регистирането на устройството!');
                }else{
                var tok1 = model.email + ':' + model.password;
                var hash1 = btoa(tok1);
                 $.ajax({
                    url: "http://bgtaxi.net/Account/LoginExternal?accessToken=" + getFromLocalStorage("accessToken")  + "&basicAuth=" +hash1+ "&requiredRoleId=2",
                    type: "POST",
                    dataType: "json",
                    contentType: "application/json",
                    success: function (status) {

                            saveInLocalStorage("accessToken", status.accessToken);
                        if (status.status == "OK") {
                            saveInLocalStorage("user", "true");
                            var userInfo = status.user.firstName + " " + status.user.lastName + " (Кола №" + status.user.carIN + ")";
                            saveInLocalStorage("userShortInfo", userInfo);
                            saveInLocalStorage("userFirstName", status.user.firstName);
                            saveInLocalStorage("userLastName", status.user.lastName);
                            saveInLocalStorage("carIN", status.user.carIN); 
                            localStorage.setItem("free", "true");
                            homeModel.email = "";
                            homeModel.password = "";
                            app.mobileApp.navigate('components/mainView/view.html');
                        } else if (status.status == "ROLE NOT MATCH"){
                               model.set('errorMessage', 'Този акаунт няма права да използва това приложение.');
                        } else if (status.status == "USER LOGGED IN"){
                               model.set('errorMessage', 'Този акаунт се използва от друго устройство.');
                        }else if (status.status == "NO COMPANY"){
                               model.set('errorMessage', 'Този акаунт не отговаря на фирма.');
                               app.mobileApp.navigate('components/companyCodeView/view.html');
                        }else if (status.status == "NO CAR"){
                               model.set('errorMessage', 'Този акаунт няма съответстващ автомобил');
                        }else if (status.status == "NO EMAIL"){
                               model.set('errorMessage', 'Този акаунт не е активиран. Проверете вашата поща');
                        }else if(status.status == "ERR"){
                                model.set('errorMessage', 'Не беше намерен акаунт с посочените имейл и парола');
                        }else{
                            init();
                        }
                    },
                    error: function () {
                        $("#messageBox").html("Error");
                        alert("error comunicating with serveer");
                    }
                });
                }
            },
            register: function() {
                var model = homeModel,
                    email = model.email.toLowerCase(),
                    password = model.password,
                    displayName = model.displayName,
                    attrs = {
                        Email: email,
                        DisplayName: displayName
                    };

                if (!model.validateData(model)) {
                    return false;
                }

               $.ajax({
                    url: "http://peter200195-001-site1.btempurl.com/Account/RegisterExternal?email=" + email + "&password=" + password + "&dataRole=Driver",
                    type: "POST",
                    dataType: "json",
                    contentType: "application/json",
                    success: function (status) {
                        if (status.status == "OK") {
                            alert("success");
                            var rememberedData = {
                                email: email,
                                password: password,
                                result: displayName,
                                rememberme: true
                            };
                            app.mobileApp.navigate('components/mainView/view.html');
                        } else {
                            init();
                        }
                    },
                    error: function () {
                        $("#messageBox").html("Error");
                        alert("error comunicating with serveer");
                    }
               

            });

            },
            toggleView: function() {
                var model = homeModel;
                model.set('errorMessage', '');

                mode = mode === 'signin' ? 'register' : 'signin';

                init();
            }
        });

    parent.set('homeModel', homeModel);
    parent.set('afterShow', function(e) {
        if (e && e.view && e.view.params && e.view.params.logout) {
            homeModel.set('logout', true);
        }
    });
})(app.home);

function getFromLocalStorage(key) {
    if (localStorage) {
        return localStorage.getItem(key);
    } else {
        return app[key];
    }
}
function saveInLocalStorage(key, info) {
    if (localStorage) {
        localStorage.setItem(key, info);
    } else {
        app[key] = info;
    }
}
function startWorker(){
   //localStorage.setItem("accessToken", "83744eae-a0b6-44cc-ac38-a0702d20a623");
    var w = new Worker("device_register.js");
             w.onmessage = function(event) {
           localStorage.setItem("accessToken", event.data);
                app["accessToken"] = event.data
                w.terminate();
                w = undefined;
        };
}
// START_CUSTOM_CODE_homeModel
// Add custom code here. For more information about custom code, see http://docs.telerik.com/platform/screenbuilder/troubleshooting/how-to-keep-custom-code-changes

// END_CUSTOM_CODE_homeModel