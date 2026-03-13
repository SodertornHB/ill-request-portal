function updatematerialTypeSelectFields() {
    const materialType = $('#materialTypeSelect').val();

    $('.field-book, .field-article, .field-chapter, .bibliographicLookupStatus').hide();

    if (materialType === 'Book') {
        $('.field-book').show();
    }
    else if (materialType === 'Article') {
        $('.field-article').show();
    }
    else if (materialType === 'Chapter') {
        $('.field-chapter').show();
    }

    $('#bibliographicLookupStatus')
        .addClass('hidden lookup-status-neutral lookup-error')
        .removeClass('lookup-status-success');
}
function setBibliographicFieldsDisabled(disabled) {
    $('#Isbn').prop('disabled', disabled);
    $('#Issn').prop('disabled', disabled);
    $('#MainTitle').prop('disabled', disabled);
    $('#MainAuthor').prop('disabled', disabled);
    $('#BookTitle').prop('disabled', disabled);
    $('#BookAuthor').prop('disabled', disabled);
    $('#BookPublicationYear').prop('disabled', disabled);
    $('#ContainerTitle').prop('disabled', disabled);
    $('#ArticlePublicationYear').prop('disabled', disabled);
    $('#ArticleAuthor').prop('disabled', disabled);
    $('#ArticleTitle').prop('disabled', disabled);
    $('#ChapterPages').prop('disabled', disabled);
    $('#ChapterBookTitle').prop('disabled', disabled);
    $('#Volume').prop('disabled', disabled);
    $('#Issue').prop('disabled', disabled);
    $('#ArticlePages').prop('disabled', disabled);
    $('#JournalTitle').prop('disabled', disabled);
    $('#materialTypeSelect').prop('disabled', disabled);
}

function populateBibliographicFields(data) {
    const materialType = $('#materialTypeSelect').val();

    if (materialType === 'Book' || materialType === 'Chapter') {

        $('#BookTitle').val(data.title || '');
        $('#BookAuthor').val(data.author || '');
        $('#BookPublicationYear').val(data.publicationYear || '');

    }

    if (materialType === 'Chapter') {

        $('#ChapterTitle').val('');
        $('#BookTitle').val(data.title || '');
        $('#ChapterBookAuthor').val(data.author || '');
        $('#Pages').val(data.pages || '');
        $('#BookPublicationYear').val(data.publicationYear || '');
    }

    if (materialType === 'Article') {

        $('#JournalTitle').val(data.journalTitle || data.title || '');
        $('#ArticleTitle').val('');
        $('#ArticleAuthor').val(data.author || '');

        $('#Volume').val('');
        $('#Issue').val(data.issue || '');
        $('#ArticlePublicationYear').val(data.publicationYear || '');
        $('#Pages').val(data.pages || '');

    }
    
}
function getStandardNumber() {
    const materialType = $('#materialTypeSelect').val();

    if (materialType === 'Article') {
        return $('#Issn').val();
    }

    return $('#Isbn').val();
}
function getQueryField() {
    const materialType = $('#materialTypeSelect').val();

    if (materialType === 'Article') {
        return 'issn';
    }
    return 'isbn';
}

function bindBibliographicLookup(texts) {
    $('#Isbn, #Issn').on('change', function () {
        const materialType = $('#materialTypeSelect').val();
        console.log(materialType);
        const standardNumber = getStandardNumber();
        const queryField = getQueryField();

        if (!materialType)
            return;

        if (!standardNumber)
            return;

        const $field = $(this);

        if (!$field.valid())
            return;

        setBibliographicFieldsDisabled(true);

        $('#bibliographicLookupSpinner').removeClass('hidden');

        $('#bibliographicLookupStatus')
            .removeClass('hidden lookup-status-success lookup-error')
            .addClass('lookup-status-neutral')
            .text(texts.lookingUpBibliographicRecord);

        $.get('/api/v1/bibliographic-records/lookup?standardNumber=' + encodeURIComponent(standardNumber) + '&queryField=' + encodeURIComponent(queryField))
            .done(function (data) {

                const $status = $('#bibliographicLookupStatus');

                if (data.status === 'FoundInKoha' || data.status === 'FoundInLibris') {
                    populateBibliographicFields(data);
                }

                if (data.status === 'FoundInKoha') {

                    const biblioId = data.biblioId || data.BiblioId || data.biblio_id;
                    const template = texts.discoveryRecordUrlTemplate;

                    if (biblioId) {
                        $status
                            .removeClass('hidden lookup-status-neutral lookup-error')
                            .addClass('lookup-status-success');

                        if (template) {
                            const discoveryUrl = template.replace('{biblioId}', biblioId);
                            $status.html(`${texts.foundInKohaMessage} <a target="_blank" href="${discoveryUrl}">${texts.kohaBorrowLinkText}</a>`);
                        } else {
                            $status.text(texts.foundInKohaMessage);
                        }
                    } else {
                        $status
                            .removeClass('hidden lookup-status-neutral lookup-ok')
                            .addClass('lookup-error')
                            .text(texts.missingBiblioIdMessage);
                    }
                }
                else if (data.status === 'FoundInLibris') {
                    $status
                        .removeClass('hidden lookup-status-neutral lookup-error')
                        .addClass('lookup-status-success')
                        .text(texts.foundInLibrisMessage);
                }
                else {
                    $status
                        .removeClass('hidden lookup-status-neutral lookup-ok')
                        .addClass('lookup-error')
                        .text(texts.noBibliographicRecordFoundMessage);
                }
            })
            .fail(function () {
                $('#bibliographicLookupStatus')
                    .removeClass('hidden lookup-status-neutral lookup-ok')
                    .addClass('lookup-error')
                    .text(texts.librarySystemErrorMessage);
            })
            .always(function () {
                setBibliographicFieldsDisabled(false);
                $('#bibliographicLookupSpinner').addClass('hidden');
            });
    });
}
$(document).ready(function () {

    const texts = window.illTexts;

    updatematerialTypeSelectFields();
    $('#materialTypeSelect').on('change', function () {
        updatematerialTypeSelectFields();
    });

    $('#CardNumber').on('change', function () {

        const $field = $(this);
        const cardNumber = $field.val();

        if (!cardNumber)
            return;

        if (!$field.valid())
            return;

        $('#RequesterName').prop('disabled', true);
        $('#RequesterEmail').prop('disabled', true);

        $('#patronLookupSpinner').removeClass('hidden');

        $('#patronLookupStatus')
            .removeClass('hidden lookup-status-success lookup-error')
            .addClass('lookup-status-neutral')
            .text(texts.lookingUpPatron);

        $.get('/api/v1/patrons?cardNumber=' + encodeURIComponent(cardNumber))

            .done(function (data) {

                $('#RequesterName').val(((data.firstname || '') + ' ' + (data.surname || '')).trim());
                $('#RequesterEmail').val(data.email || '');

                $('#patronLookupStatus')
                    .removeClass('lookup-status-neutral lookup-error')
                    .addClass('lookup-status-success')
                    .text(texts.patronFound);

            })

            .fail(function (xhr) {

                $('#RequesterName').val('');
                $('#RequesterEmail').val('');

                if (xhr.status === 404) {
                    $('#patronLookupStatus')
                        .removeClass('lookup-status-neutral lookup-ok')
                        .addClass('lookup-error')
                        .text(texts.noPatronFound);
                }
                else {
                    $('#patronLookupStatus')
                        .removeClass('lookup-status-neutral lookup-ok')
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

    bindBibliographicLookup(texts)    
});

