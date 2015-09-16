/// <reference path="jquery-2.1.0.min.js" />

function requestLongUrl(shortUrl, requestUrl, resultSelector, outputSelector) {
    $.ajax({
        url: requestUrl,
        dataType: "json",
        async: true,
        jsonp: false,
        type: "POST",
        data: {
            shortUrl: shortUrl,
            ts: new Date().getTime()

        },
        error: function (xhr, textStatus, errorThrown) {
            alert("Error: " + errorThrown);
            $(resultSelector).append(errorThrown);
        },
        success: function (data) {
            $(resultSelector).text("");
            if (data.status == true) {
                $(resultSelector).append('<a href="' + data.url.LongUrl + '" target="_blank">' + data.url.LongUrl + '</a>');
            } else {
                $(resultSelector).append(data.message);
            }
            $(outputSelector).slideDown('slow');
        }
    });
}