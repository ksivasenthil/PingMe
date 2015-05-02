$(document).ready(function () {
    pingBehaviors.loadConversations();
    $(document).on('keydown', function (e) {
        if (e.keyCode === 27) { // ESC
            pingBehaviors.loadConversations();
        }
    });
    $('#btnRefresh').on('click', function () {
        pingBehaviors.loadConversations();
    });
    //$(document.body).on('click', '#detailPings', function () {
    //    alert('called');
    //    var source = $(this).attr('data-Source');
    //    var destination = $(this).attr('data-Destination');
    //    pingBehaviors.loadMessages(source, destination);
    //    return false;
    //});
    $(document.body).on('click', '#message_send', function () {
        pingBehaviors.pingMessage();
    });
});
var pingBehaviors = (function () {
    var baseUrl = "http://localhost:61982/MessengerServiceFacade.svc";
    //baseUrl: "http://localhost/FamilyMessenger/MessengerServiceFacade.svc",
    var universalSource = "+919840200524";
    var arrayNamespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays";
    var messageNamespace = 'http://schemas.datacontract.org/2004/07/MessagingEntities';
    var TN_MESSAGE_RESULT = "FetchMessagesResult";
    var TN_MESSAGE_STRUCTURE = "MessagePing";
    var TN_MESSAGE_DESTINED_PINGER_PROFILE = 'DestinedUserProfile';
    var TN_MESSAGE_DESTINED_PINGER_IMAGE = 'PingerImage';
    var TN_MESSAGE_DESTINED_PINGER = 'Destination';
    var TN_MESSAGE_ID = "Id";
    var TN_MESSAGE_TEXT = "Message";
    var TN_MESSAGE_SOURCE = "Source";;
    var TN_MESSAGE_SENT_UTC = "MessageSentUTC";
    var TN_MESSAGE_RECIEVED_UTC = "MessageRecievedUTC";
    var TN_CONVERSATION_RESULT = 'ConversationResult';
    var TN_PING_LIST = 'PingList';
    var TN_PINGER_DESTINATION_PROFILE = 'DestinationPingerProfile';
    var TN_PINGER_SOURCE = 'PingerSource';
    var TN_PINGER_PROFILE_IMAGE = 'PingerImage';
    var TN_PINGER_DESTINATION = 'PingerDestination';
    var TN_PINGER_LAST_MESSAGE = 'LastMessage';
    var padNumber = function (param, numberOfZeroes) {
        numberOfZeroes = (typeof numberOfZeroes === 'undefined') ? 1 : numberOfZeroes;
        var paddedZeroes = "";
        for (var i = 0; i < numberOfZeroes; i++) {
            paddedZeroes += "0";
        }
        return param < 10 ? paddedZeroes + param : param;
    };
    var getLocalTimeFromUTC = function (utcTime) {
        utcTime = utcTime.replace('T', ' ').replace(/-/g, '/');
        utcTime = utcTime.substring(0, utcTime.indexOf('.'))
        utcTime += ' UTC';
        return new Date(utcTime);
    };
    var getTimeStampPrintFormat = function (timeStamp) {
        return padNumber(timeStamp.getDate()) + "/" + padNumber(timeStamp.getMonth() + 1) + "/" + padNumber(timeStamp.getYear() - 100) + " " + padNumber(timeStamp.getHours()) + ":" + padNumber(timeStamp.getMinutes())
    };
    var setUniversalSource = function () {
        universalSource = $('#sourcePinger').val() ? $('#sourcePinger').val() : universalSource;
    };
    return {
        loadConversations: function () {
            setUniversalSource();
            var soapRequest = '<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"><s:Body><Conversation xmlns="http://vosspace.com/FamilyConnect/MessengerService"><source>[Source]</source></Conversation></s:Body></s:Envelope>';
            soapRequest = soapRequest.replace('[Source]', universalSource);
            var template = null;
            var formattedMarkup = null;
            $.ajax({
                url: baseUrl,
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
                        var conversationResult = $(xmlHttpResponse.responseXML).find(TN_CONVERSATION_RESULT)[0];
                        var conversations = conversationResult.getElementsByTagNameNS(messageNamespace, TN_PING_LIST);
                        $(conversations).each(function (index) {
                            var pingerProfile = this.getElementsByTagNameNS(messageNamespace, TN_PINGER_DESTINATION_PROFILE)[0];
                            var pingerSource = $(this.getElementsByTagNameNS(messageNamespace, TN_PINGER_SOURCE)[0]).text();
                            var pingerDestination = $(this.getElementsByTagNameNS(messageNamespace, TN_PINGER_DESTINATION)[0]).text();
                            var pingerImage = $(pingerProfile.getElementsByTagNameNS(messageNamespace, TN_PINGER_PROFILE_IMAGE)[0]).text();
                            var pingerLastMessage = $(pingerProfile.getElementsByTagNameNS(messageNamespace, TN_PINGER_LAST_MESSAGE)[0]).text();

                            var shouldIAdd = '' != pingerDestination && null !== pingerDestination;
                            if (shouldIAdd) {
                                displayConversation.pings.push({
                                    "Source": universalSource == pingerSource ? pingerSource : pingerDestination,
                                    "Destination": universalSource == pingerSource ? pingerDestination : pingerSource,
                                    "last_message": pingerLastMessage,
                                    "ProfileImage": pingerImage
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
            var source = universalSource;
            var target = $(txtPingTarget).val();
            var message = $(txtPing).val();

            this.postPing(source, target, message);
        },
        loadMessages: function (left, right) {
            var source = left;
            var target = right;
            var template = null;
            var formattedMarkup = null;

            var soapRequest = '<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"><s:Body><FetchMessages xmlns="http://vosspace.com/FamilyConnect/MessengerService"><source>[Source]</source><destination>[Destination]</destination></FetchMessages></s:Body></s:Envelope>';
            soapRequest = soapRequest.replace('[Destination]', target).replace('[Source]', source);
            $.ajax({
                url: baseUrl,
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
                        var messagesResult = $(xmlHttpResponse.responseXML).find(TN_MESSAGE_RESULT)[0];
                        var allMessages = messagesResult.getElementsByTagNameNS(messageNamespace, TN_MESSAGE_STRUCTURE);
                        var displayMessage = {
                            "pings": [],
                            "profile": []
                        };
                        var profileIfNeverPinged = '';
                        $(allMessages).each(function (index) {
                            var shouldIAdd = 0 < $(this).children().length;
                            if (shouldIAdd) {
                                var message_id = $(this.getElementsByTagNameNS(messageNamespace, TN_MESSAGE_ID)[0]).text();
                                var message = $(this.getElementsByTagNameNS(messageNamespace, TN_MESSAGE_TEXT)[0]).text();
                                var source = $(this.getElementsByTagNameNS(messageNamespace, TN_MESSAGE_SOURCE)[0]).text();
                                var timeSent = getLocalTimeFromUTC($(this.getElementsByTagNameNS(messageNamespace, TN_MESSAGE_SENT_UTC)[0]).text());
                                var timeRecieved = getLocalTimeFromUTC($(this.getElementsByTagNameNS(messageNamespace, TN_MESSAGE_RECIEVED_UTC)[0]).text());

                                displayMessage.pings.push({
                                    "message_id": message_id,
                                    "message": message,
                                    "participant_direction": (source.trim() == left.trim()) ? "flat_layout_left" : "flat_layout_right",
                                    "sent_time": isNaN(timeSent.getDate()) ? '*' : getTimeStampPrintFormat(timeSent),
                                    "recieved_time": isNaN(timeRecieved.getDate()) ? '*' : getTimeStampPrintFormat(timeRecieved)
                                });

                                var destinedPingerProfile = this.getElementsByTagNameNS(messageNamespace, TN_MESSAGE_DESTINED_PINGER_PROFILE)[0];
                                var pingerProfile = $(destinedPingerProfile.getElementsByTagNameNS(messageNamespace, TN_MESSAGE_DESTINED_PINGER_IMAGE)[0]).text();
                                var destinationPinger = $(this.getElementsByTagNameNS(messageNamespace, TN_MESSAGE_DESTINED_PINGER)[0]).text();
                                var shouldISetDestinedUserProfile = 0 >= displayMessage.profile.length && right == destinationPinger;
                                if (shouldISetDestinedUserProfile) {
                                    displayMessage.profile.push({
                                        "profile_picture": pingerProfile,
                                        "destined_user": destinationPinger
                                    });
                                }
                                profileIfNeverPinged = pingerProfile;
                            }
                        });
                        var stillEmptyProfile = 0 >= displayMessage.profile.length;
                        if (stillEmptyProfile) {
                            displayMessage.profile.push({
                                "profile_picture": profileIfNeverPinged,
                                "destined_user": right
                            });
                        }

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
        },
        postPing: function (source, target, message) {
            var soapRequest = '<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"><s:Body><PostMessage xmlns="http://vosspace.com/FamilyConnect/MessengerService"><messageDetails xmlns:a="http://schemas.datacontract.org/2004/07/MessagingEntities" xmlns:i="http://www.w3.org/2001/XMLSchema-instance"><a:Id>00000000-0000-0000-0000-000000000000</a:Id><a:Asset i:nil="true"/><a:Destination>[Destination]</a:Destination><a:Message>[Message]</a:Message><a:MessageRecievedUTC i:nil="true"/><a:MessageSentUTC i:nil="false">[SentUTC]</a:MessageSentUTC><a:Source>[Source]</a:Source></messageDetails></PostMessage></s:Body></s:Envelope>';
            var now = new Date();
            soapRequest = soapRequest.replace('[Destination]', target).replace('[Source]', source).replace('[Message]', message).replace('[SentUTC]', now.getUTCFullYear() + "-" + padNumber(now.getUTCMonth() + 1) + "-" + padNumber(now.getUTCDate()) + "T" + padNumber(now.getUTCHours()) + ":" + padNumber(now.getUTCMinutes()) + ":" + padNumber(now.getUTCSeconds()) + "." + padNumber(now.getUTCMilliseconds(), 2));
            $.ajax({
                url: baseUrl,
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
        pingMessage: function () {
            var target = $('#destinedPinger').text().trim();
            var message = $('#message_text').val();
            this.postPing(universalSource, target, message);
            this.loadMessages(universalSource, target);
        }
    };
})();