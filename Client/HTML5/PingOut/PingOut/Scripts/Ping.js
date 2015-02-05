$(document).ready(function () {
    pingBehaviors.loadConversations();
    $(document).on('keydown', function (e) {
        if (e.keyCode === 27) { // ESC
            pingBehaviors.loadConversations();
        }
    });
});
var pingBehaviors = {
    loadConversations: function () {
        var soapRequest = '<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"><s:Body><Conversation xmlns="http://vosspace.com/FamilyConnect/MessengerService"><source>+919840200524</source></Conversation></s:Body></s:Envelope>';
        var template = null;
        var formattedMarkup = null;
        $.ajax({
            url: "http://localhost:61982/MessengerServiceFacade.svc",
            headers: { "SOAPAction": '"http://vosspace.com/FamilyConnect/MessengerService/IMessengerServiceFacade/Conversation"' },
            type: "POST",
            dataType: "xml",
            data: soapRequest,
            complete: function (xmlHttpResponse, status) {
                var displayConversation = {
                    "pings": []

                };
                var conversationResult = $(xmlHttpResponse.responseXML).find('ConversationResult')[0];
                var conversations = conversationResult.getElementsByTagNameNS('http://schemas.microsoft.com/2003/10/Serialization/Arrays', 'string');
                $(conversations).each(function (index) {
                    var shouldIAdd = '' != $(this).text() && null !== $(this).text();
                    if (shouldIAdd) {
                        displayConversation.pings.push({
                            "Source": $(this).text(),
                            "last_message": "..."
                        });
                    }
                });
                var areThereConversations = 0 < displayConversation.pings.length;
                if (areThereConversations) {
                    template = $(conversationsList).html();
                    formattedMarkup = Mustache.render(template, displayConversation);
                } else {
                    template = $(newPing).html();
                    formattedMarkup = Mustache.render(template, displayConversation);
                }
                $('#contentHost').html(formattedMarkup);
            },
            contentType: "text/xml; charset=\"utf-8\""
        });
    },
    invokeNewConversation: function () {
        var template = null;
        var formattedMarkup = null;
        template = $(newPing).html();
        formattedMarkup = Mustache.render(template, null);
        $('#contentHost').html(formattedMarkup);
    }
};