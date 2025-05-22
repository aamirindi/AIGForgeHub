$(document).ready(function () {
    // alert('Hello');
});



$('#loginForm').submit(function (e) {
    e.preventDefault();

    var obj = $("#loginForm").serialize();

    $.ajax({
        url: "/Auth/Login",
        type: 'POST',
        dataType: 'json',
        data: obj,
        success: function (res) {
            console.log(res);
            if (res.success == true) {
                toastr.success(res.message);
                window.location.href = res.redirectUrl;
            }
            else {
                toastr.error(res.message);
            }
            $('#loginForm')[0].reset();
        },
        error: function (xhr, status, error) {
            console.error(xhr.responseText);
            alert("An error occurred. Please try again.");
            toastr.warning(error);
        }
    });
});