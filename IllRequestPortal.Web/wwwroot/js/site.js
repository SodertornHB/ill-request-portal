function getBaseUrl() {
    let url = new URL(window.location.href);
    let pathname = getPathName(url);
    url = `${url.protocol}//${url.host}${pathname}`;
    if (url.substring(url.length - 1) === "/") {
        url = url.slice(0, -1);
    }
    url += "/resources";
    return url;
}

function getPathName(url) {
    let pathName = url.pathname;
    let pathNames = pathName.split('/');
    if (pathNames.length > 2) {
        pathName = '/' + pathNames[1];
    }
    return pathName;
};
document.addEventListener('DOMContentLoaded', function () {
    $('.tooltip-r').tooltip();   
    $('.icon-with-text').on('click', function () {
        $('#changeLanguageFormId').submit();
    })
    let url = getBaseUrl();
    $.get(url, function (json) {
        let data = JSON.parse(json);
        $('.data-table').DataTable({
            'language': {
                'url': data.TableLang
            },
            "paging": false,
            "order": [],
            dom: 'Bfrtip',
            buttons: [
                { extend: 'copy',  className: 'btn btn-secondary btn-sm table-button' },
                { extend: 'excel', className: 'btn btn-secondary btn-sm table-button' },
                { extend: 'csv',   className: 'btn btn-secondary btn-sm table-button' },
                { extend: 'print', className: 'btn btn-secondary btn-sm table-button' }
            ],
            initComplete: function () {
                $(".table-button").removeClass("buttons-html5").removeClass("buttons-copy").removeClass("dt-button");
            },

        });
    });
});