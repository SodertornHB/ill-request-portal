$(document).ready(function () {

    $('#CardNumber').on('change', function () {

        var cardNumber = $(this).val();

        if (!cardNumber)
            return;

        $('#RequesterName').prop('disabled', true);
        $('#RequesterEmail').prop('disabled', true);

        $('#patronLookupSpinner').show();
        $('#patronLookupStatus')
            .removeClass('lookup-error lookup-ok')
            .text('Looking up patron...');

        $.get('/api/v1/patrons?cardNumber=' + encodeURIComponent(cardNumber))

            .done(function (data) {

                $('#RequesterName').val(((data.firstname || '') + ' ' + (data.surname || '')).trim());
                $('#RequesterEmail').val(data.email || '');

                $('#patronLookupStatus')
                    .addClass('lookup-ok')
                    .text('Patron found');

            })

            .fail(function (xhr) {

                $('#RequesterName').val('');
                $('#RequesterEmail').val('');

                if (xhr.status === 404) {
                    $('#patronLookupStatus')
                        .addClass('lookup-error')
                        .text('No patron found with that card number');
                }
                else {
                    $('#patronLookupStatus')
                        .addClass('lookup-error')
                        .text('Could not contact library system');
                }

            })

            .always(function () {

                $('#RequesterName').prop('disabled', false);
                $('#RequesterEmail').prop('disabled', false);

                $('#patronLookupSpinner').hide();

            });

    });

});