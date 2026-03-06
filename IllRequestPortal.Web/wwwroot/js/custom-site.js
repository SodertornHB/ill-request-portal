$(document).ready(function () {

    $('#CardNumber').on('change', function () {

        var cardNumber = $(this).val();

        if (!cardNumber) {
            return;
        }

        $.get('/api/v1/patrons?cardNumber=' + encodeURIComponent(cardNumber))
            .done(function (data) {

                $('#RequesterName').val((data.firstname || '') + ' ' + (data.surname || ''));
                $('#RequesterEmail').val(data.email || '');
                $('#CardNumber').val(data.cardnumber || cardNumber);

            })
            .fail(function () {

                $('#RequesterName').val('');
                $('#RequesterEmail').val('');
                console.log('Could not fetch patron');

            });

    });

});