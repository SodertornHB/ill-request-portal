$(document).ready(function () {

    const texts = window.illTexts;

    $('#CardNumber').on('change', function () {

        var cardNumber = $(this).val();

        if (!cardNumber)
            return;

        $('#RequesterName').prop('disabled', true);
        $('#RequesterEmail').prop('disabled', true);

        $('#patronLookupSpinner').removeClass('hidden');

        $('#patronLookupStatus')
            .removeClass('lookup-error lookup-ok')
            .text(texts.lookingUpPatron);

        $.get('/api/v1/patrons?cardNumber=' + encodeURIComponent(cardNumber))

            .done(function (data) {

                $('#RequesterName').val(((data.firstname || '') + ' ' + (data.surname || '')).trim());
                $('#RequesterEmail').val(data.email || '');

                $('#patronLookupStatus')
                    .removeClass('lookup-error')
                    .addClass('lookup-ok')
                    .text(texts.patronFound);

            })

            .fail(function (xhr) {

                $('#RequesterName').val('');
                $('#RequesterEmail').val('');

                if (xhr.status === 404) {
                    $('#patronLookupStatus')
                        .removeClass('lookup-ok')
                        .addClass('lookup-error')
                        .text(texts.noPatronFound);
                }
                else {
                    $('#patronLookupStatus')
                        .removeClass('lookup-ok')
                        .addClass('lookup-error')
                        .text(texts.librarySystemErrorMessage);
                }

            })

            .always(function () {

                $('#RequesterName').prop('disabled', false);
                $('#RequesterEmail').prop('disabled', false);

                $('#patronLookupSpinner').addClass('hidden');

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

        $('#bibliographicLookupSpinner').removeClass('hidden');

        $('#bibliographicLookupStatus')
            .removeClass('lookup-error lookup-ok')
            .text(texts.lookingUpBibliographicRecord);

        $.get('/api/v1/bibliographic-records/lookup?standardNumber=' + encodeURIComponent(standardNumber))

            .done(function (data) {

                const $status = $('#bibliographicLookupStatus');

                $status
                    .removeClass('lookup-ok lookup-error')
                    .text('');

                if (data.status === 'FoundInKoha' || data.status === 'FoundInLibris') {

                    $('#Title').val(data.title || '');
                    $('#Author').val(data.author || '');
                    $('#PublicationYear').val(data.publicationYear || '');
                    $('#Edition').val(data.edition || '');

                }

                if (data.status === 'FoundInKoha') {

                    const biblioId = data.biblioId || data.BiblioId || data.biblio_id;
                    const template = texts.discoveryRecordUrlTemplate;

                    if (biblioId) {

                        $status
                            .removeClass('lookup-error')
                            .addClass('lookup-ok');

                        if (template) {

                            const discoveryUrl = template.replace('{biblioId}', biblioId);

                            $status.html(
                                `${texts.foundInKohaMessage} <a target="_blank" href="${discoveryUrl}">${texts.kohaBorrowLinkText}</a>`
                            );

                        } else {

                            $status.text(texts.foundInKohaMessage);

                        }

                    } else {

                        $status
                            .removeClass('lookup-ok')
                            .addClass('lookup-error')
                            .text(texts.missingBiblioIdMessage);

                    }

                }
                else if (data.status === 'FoundInLibris') {
                    $status
                        .removeClass('lookup-error')
                        .addClass('lookup-ok')
                        .text(texts.foundInLibrisMessage);
                }
                else {
                    $status
                        .removeClass('lookup-ok')
                        .addClass('lookup-error')
                        .text(texts.noBibliographicRecordFoundMessage);
                }

            })

            .fail(function () {

                $('#bibliographicLookupStatus')
                    .removeClass('lookup-ok')
                    .addClass('lookup-error')
                    .text(texts.librarySystemErrorMessage);

            })

            .always(function () {

                $('#Title').prop('disabled', false);
                $('#Author').prop('disabled', false);
                $('#PublicationYear').prop('disabled', false);
                $('#Edition').prop('disabled', false);
                $('#MaterialType').prop('disabled', false);

                $('#bibliographicLookupSpinner').addClass('hidden');

            });

    });

});