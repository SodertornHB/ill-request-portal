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

    $('#IsbnIssn').on('change', function () {

        var standardNumber = $(this).val();

        if (!standardNumber)
            return;

        $('#Title').prop('disabled', true);
        $('#Author').prop('disabled', true);
        $('#PublicationYear').prop('disabled', true);
        $('#Edition').prop('disabled', true);
        $('#MaterialType').prop('disabled', true);

        $('#bibliographicLookupSpinner').show();

        $('#bibliographicLookupStatus')
            .removeClass('lookup-error lookup-ok')
            .text('Looking up bibliographic record...');

        $.get('/api/v1/bibliographic-records/lookup?standardNumber=' + encodeURIComponent(standardNumber))

            .done(function (data) {

                if (data.status === 'FoundInKoha') {

                    $('#bibliographicLookupStatus')
                        .addClass('lookup-ok')
                        .text('Item already exists in Koha');

                }

                else if (data.status === 'FoundInLibris') {

                    $('#Title').val(data.title || '');
                    $('#Author').val(data.author || '');
                    $('#PublicationYear').val(data.publicationYear || '');
                    $('#Edition').val(data.edition || '');
                    $('#MaterialType').val(data.materialType || '');

                    $('#bibliographicLookupStatus')
                        .addClass('lookup-ok')
                        .text('Bibliographic information found in Libris');

                }

                else {

                    $('#bibliographicLookupStatus')
                        .text('No record found in Koha or Libris');

                }

            })

            .fail(function () {

                $('#bibliographicLookupStatus')
                    .addClass('lookup-error')
                    .text('Could not contact library system');

            })

            .always(function () {

                $('#Title').prop('disabled', false);
                $('#Author').prop('disabled', false);
                $('#PublicationYear').prop('disabled', false);
                $('#Edition').prop('disabled', false);
                $('#MaterialType').prop('disabled', false);

                $('#bibliographicLookupSpinner').hide();

            });

    });

});