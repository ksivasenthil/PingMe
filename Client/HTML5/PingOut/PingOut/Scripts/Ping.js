$(document).ready(function () {
    var data = {
        "pings": [
            {
                "Source": "1",
                "last_message": "1 list ..."
            },
            {
                "Source": "2",
                "last_message": "2 list dynamic ..."
            }
        ]

    };
    var template = $(templateHost).html();
    var formattedMarkup = Mustache.render(template, data);
    $('#contentHost').html(formattedMarkup);
});