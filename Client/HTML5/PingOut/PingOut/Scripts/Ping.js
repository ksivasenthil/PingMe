$(document).ready(function () {
    pingBehaviors.loadConversations();
    $(document).on('keydown', function (e) {
        if (e.keyCode === 27) { // ESC
            pingBehaviors.loadConversations();
        }
    });
});
var pingBehaviors = {
    baseUrl: "http://localhost:61982/MessengerServiceFacade.svc",
    //baseUrl: "http://localhost/FamilyMessenger/MessengerServiceFacade.svc",
    universalSource: "+919840200524",
    arrayNamespace: "http://schemas.microsoft.com/2003/10/Serialization/Arrays",
    messageNamespace: 'http://schemas.datacontract.org/2004/07/MessagingEntities',
    loadConversations: function () {
        var soapRequest = '<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"><s:Body><Conversation xmlns="http://vosspace.com/FamilyConnect/MessengerService"><source>[Source]</source></Conversation></s:Body></s:Envelope>';
        soapRequest = soapRequest.replace('[Source]', this.universalSource);
        var parent = this;
        var template = null;
        var formattedMarkup = null;
        $.ajax({
            url: this.baseUrl,
            crossdomain: true,
            headers: {
                "SOAPAction": '"http://vosspace.com/FamilyConnect/MessengerService/IMessengerServiceFacade/Conversation"',
            },
            type: "POST",
            dataType: "xml",
            data: soapRequest,
            processData: true,
            complete: function (xmlHttpResponse, status) {

                if (status == 'success') {
                    var displayConversation = {
                        "pings": []

                    };
                    var conversationResult = $(xmlHttpResponse.responseXML).find('ConversationResult')[0];
                    var conversations = conversationResult.getElementsByTagNameNS(parent.arrayNamespace, 'string');
                    $(conversations).each(function (index) {
                        var shouldIAdd = '' != $(this).text() && null !== $(this).text();
                        if (shouldIAdd) {
                            displayConversation.pings.push({
                                "Source": parent.universalSource,
                                "Destination": $(this).text(),
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
                }
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
    },
    startConverstion: function () {
        var source = this.universalSource;
        var target = $(txtPingTarget).val();
        var message = $(txtPing).val();

        var soapRequest = '<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"><s:Body><PostMessage xmlns="http://vosspace.com/FamilyConnect/MessengerService"><messageDetails xmlns:a="http://schemas.datacontract.org/2004/07/MessagingEntities" xmlns:i="http://www.w3.org/2001/XMLSchema-instance"><a:Id>00000000-0000-0000-0000-000000000000</a:Id><a:Asset i:nil="true"/><a:Destination>[Destination]</a:Destination><a:Message>[Message]</a:Message><a:Source>[Source]</a:Source></messageDetails></PostMessage></s:Body></s:Envelope>';
        soapRequest = soapRequest.replace('[Destination]', target).replace('[Source]', source).replace('[Message]', message);
        $.ajax({
            url: this.baseUrl,
            crossdomain: true,
            headers: {
                "SOAPAction": '"http://vosspace.com/FamilyConnect/MessengerService/IMessengerServiceFacade/PostMessage"',
            },
            type: "POST",
            dataType: "xml",
            data: soapRequest,
            processData: true,
            complete: function (xmlHttpResponse, status) {
                if (status == 'success') {
                    var postResult = $(xmlHttpResponse.responseXML).find('PostMessageResult')[0];
                    var serverPostResponse = $(postResult).text();
                    if (serverPostResponse === 'true') {
                        alert('Success !! message posted');
                    } else {
                        alert('something went wrong in server :(');
                    }
                } else {
                    alert('Not a great news :(');
                }
            },
            contentType: "text/xml; charset=\"utf-8\""
        });
    },
    loadMessages: function (left, right) {
        var source = left;
        var target = right;
        var template = null;
        var formattedMarkup = null;

        var soapRequest = '<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"><s:Body><FetchMessages xmlns="http://vosspace.com/FamilyConnect/MessengerService"><source>[Source]</source><destination>[Destination]</destination></FetchMessages></s:Body></s:Envelope>';
        soapRequest = soapRequest.replace('[Destination]', target).replace('[Source]', source);
        var parent = this;
        $.ajax({
            url: this.baseUrl,
            crossdomain: true,
            headers: {
                "SOAPAction": '"http://vosspace.com/FamilyConnect/MessengerService/IMessengerServiceFacade/FetchMessages"',
            },
            type: "POST",
            dataType: "xml",
            data: soapRequest,
            processData: true,
            complete: function (xmlHttpResponse, status) {
                if (status == 'success') {
                    var messagesResult = $(xmlHttpResponse.responseXML).find('FetchMessagesResult')[0];
                    var allMessages = messagesResult.getElementsByTagNameNS(parent.messageNamespace, 'MessagePing');
                    var displayMessage = {
                        "pings": []

                    };
                    $(allMessages).each(function (index) {
                        var shouldIAdd = 0 < $(this).children().length;
                        if (shouldIAdd) {
                            var message_id = $(this.getElementsByTagNameNS(parent.messageNamespace, 'Id')[0]).text();
                            var message = $(this.getElementsByTagNameNS(parent.messageNamespace, 'Message')[0]).text();
                            displayMessage.pings.push({
                                "message_id": message_id,
                                "message": message
                            });
                        }
                    });

                    var areThereMessages = 0 < displayMessage.pings.length;
                    if (areThereMessages) {
                        template = $(messages).html();
                        formattedMarkup = Mustache.render(template, displayMessage);
                    } else {
                        template = $(newPing).html();
                        formattedMarkup = Mustache.render(template, displayMessage);
                    }
                    $('#contentHost').html(formattedMarkup);
                } else {
                    alert('Not a great news :(');
                }
            },
            contentType: "text/xml; charset=\"utf-8\""
        });
    }
};