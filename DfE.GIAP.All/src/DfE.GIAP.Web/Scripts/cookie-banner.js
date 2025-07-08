if ($('#CookieBanner').length > 0) {
    $("#btn-accept-cookie").click(function () {
        document.cookie = $(this).attr('data-cookie-string');
        $('#CookieBanner').hide();
    });
}