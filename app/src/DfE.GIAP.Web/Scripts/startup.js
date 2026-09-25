document.body.className = ((document.body.className) ? document.body.className + ' js-enabled' : 'js-enabled');
window.GOVUKFrontend.initAll();
$('.javascript-off').css('display', 'none');

$(document).ready(function () {
    $('#toggleCheckbox').removeClass('hide-for-non-js');
    $('.error-table').hide();
});

$('#toggleErrors').on('click', () => {
    let checkedStatus = $('#toggleErrors').prop("checked");
    if (checkedStatus) {
        $('.error-table').show();
        $('.result-table').hide();
        $('.download-block').hide();
    } else {
        $('.error-table').hide();
        $('.result-table').show();
        $('.download-block').show();
    }
});