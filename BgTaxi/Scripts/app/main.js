$(document).scroll(function () {
    var y = $(this).scrollTop();
    if (y < 300) {

        $(".navbar-custom").removeClass("affix");
    }
    if (y > 300) {
        $(".navbar-custom").addClass("affix");
        $('#panel-our-uses').addClass("fadeIn");
        console.log("Write something");
    }
});

$(document).ready(function () {
    console.log("dsfdsa");
    $('.jumbotron-image').fadeIn(1000);
})