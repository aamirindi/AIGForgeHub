$(document).ready(function () {
    // alert('Hello');
});



$('#loginBtn').click(function () {
    var obj = $("#loginForm").serialize();

    // console.log(obj.toString());

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
            if (res.success == false) {

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
