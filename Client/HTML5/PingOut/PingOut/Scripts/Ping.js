$(document).ready(function () {
    pingBehaviors.loadPingList();
    $(document).on('keydown', function (e) {
        if (e.keyCode === 27) { // ESC
            clearInterval(pingBehaviors.timerForMessageCompleteRefresh);
            pingBehaviors.loadPingList();
        }
    });
    $('#btnRefresh').on('click', function () {
        clearInterval(pingBehaviors.timerForMessageCompleteRefresh);
        pingBehaviors.loadPingList();
    });
    $(document.body).on('click', '#detailPings', function () {
        var source = $(this).attr('data-Source');
        var destination = $(this).attr('data-Destination');
        pingBehaviors.listPings(source, destination, 'messages', 'contentHost');
        clearInterval(pingBehaviors.timerForMessageCompleteRefresh);
        pingBehaviors.timerForMessageCompleteRefresh = window.setInterval(function () {
            pingBehaviors.listPings(source, destination, 'pings', 'refreshRegion');
        }, 5000);
    });
    $(document.body).on('click', '#message_send', function () {
        pingBehaviors.ping();
    });
    $(document.body).on('click', '#btnStartPinging', function () {
        pingBehaviors.startPinging();
    });
    $(document.body).on('click', '#startConversation', function () {
        pingBehaviors.startNewPing();
    });
});
var pingBehaviors = (function () {
    var baseUrl = "http://localhost:61982/MessengerServiceFacade.svc";
    //baseUrl: "http://localhost/FamilyMessenger/MessengerServiceFacade.svc",
    var universalSource = "+919840200524";
    var arrayNamespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays";
    var pingNamespace = 'http://schemas.datacontract.org/2004/07/MessagingEntities';
    var TN_PINGS_RESULT = "FetchMessagesResult";
    var TN_PING_STRUCTURE = "MessagePing";
    var TN_PING_DESTINED_PINGER_PROFILE = 'DestinedUserProfile';
    var TN_PING_DESTINED_PINGER_IMAGE = 'PingerImage';
    var TN_PING_DESTINED_PINGER = 'Destination';
    var TN_PING_ID = "Id";
    var TN_PING_TEXT = "Message";
    var TN_PING_SOURCE = "Source";;
    var TN_PING_SENT_UTC = "MessageSentUTC";
    var TN_PING_RECIEVED_UTC = "MessageRecievedUTC";
    var TN_TARGET_LIST_RESULT = 'ConversationResult';
    var TN_PING_LIST = 'PingList';
    var TN_PINGER_DESTINATION_PROFILE = 'DestinationPingerProfile';
    var TN_PINGER_SOURCE = 'PingerSource';
    var TN_PINGER_PROFILE_IMAGE = 'PingerImage';
    var TN_PINGER_DESTINATION = 'PingerDestination';
    var TN_PINGER_LAST_MESSAGE = 'LastMessage';
    var TN_POST_PING_RESULT = 'PostMessageResult';
    var timerForMessageCompleteRefresh = null;
    var AJAX_CALL_SUCCESS_STRING = 'success';
    var HTTP_POST_CONTENT_TYPE = "text/xml; charset=\"utf-8\"";
    var HTTP_XML_DATA_DIRECTIVE = "xml";
    var HTTP_POST_DIRECTIVE = "POST";
    var SOAP_ACTION_DIRECTIVE = "SOAPAction";
    var SOAP_REQUEST_PING_LIST = '<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"><s:Body><Conversation xmlns="http://vosspace.com/FamilyConnect/MessengerService"><source>[Source]</source></Conversation></s:Body></s:Envelope>';
    var SOAP_REQUEST_FETCH_PINGS = '<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"><s:Body><FetchMessages xmlns="http://vosspace.com/FamilyConnect/MessengerService"><source>[Source]</source><destination>[Destination]</destination></FetchMessages></s:Body></s:Envelope>';
    var SOAP_REQUEST_POST_PING = '<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"><s:Body><PostMessage xmlns="http://vosspace.com/FamilyConnect/MessengerService"><messageDetails xmlns:a="http://schemas.datacontract.org/2004/07/MessagingEntities" xmlns:i="http://www.w3.org/2001/XMLSchema-instance"><a:Id>00000000-0000-0000-0000-000000000000</a:Id><a:Asset i:nil="true"/><a:Destination>[Destination]</a:Destination><a:Message>[Message]</a:Message><a:MessageRecievedUTC i:nil="true"/><a:MessageSentUTC i:nil="false">[SentUTC]</a:MessageSentUTC><a:Source>[Source]</a:Source></messageDetails></PostMessage></s:Body></s:Envelope>';
    var SOAP_ACTION_PING_LIST = "http://vosspace.com/FamilyConnect/MessengerService/IMessengerServiceFacade/Conversation";
    var SOAP_ACTION_FETCH_PINGS = "http://vosspace.com/FamilyConnect/MessengerService/IMessengerServiceFacade/FetchMessages";
    var SOAP_ACTION_POST_PING = "http://vosspace.com/FamilyConnect/MessengerService/IMessengerServiceFacade/PostMessage";
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
    var getFirstElementData = function (doc, namespace, tag) {
        return getElementData(doc, namespace, tag, 0);
    };
    var getElementData = function (doc, namespace, tag, index) {
        return $(doc.getElementsByTagNameNS(namespace, tag)[index]).text();
    };
    var getFirstElement = function (doc, namespace, tag) {
        return getElement(doc, namespace, tag, 0);
    };
    var getElement = function (doc, namespace, tag, index) {
        if (-1 == index) {
            return doc.getElementsByTagNameNS(namespace, tag);
        } else {
            return doc.getElementsByTagNameNS(namespace, tag)[index];
        }
    };
    var updateMarkup = function (tempateParams, targetRegion, alternateTargetRegion, hostRegion, updateConditions) {
        var canUpdateMarkup = true;
        $(updateConditions).each(function () {
            canUpdateMarkup &= 0 < tempateParams[this].length;
        });
        var template = null;
        var formattedMarkup = null;
        if (canUpdateMarkup) {
            template = $('#' + targetRegion + '').html();
            formattedMarkup = Mustache.render(template, tempateParams);
        } else {
            template = $('#' + alternateTargetRegion + '').html();
            formattedMarkup = Mustache.render(template, tempateParams);
        }
        $('#' + hostRegion + '').html(formattedMarkup);
    }
    var callServerAsync = function (baseUrl, soapAction, httpDirective, httpDataType, contentType,
                                    soapRequest, completeEventHandler, callbackParams, additionalParams) {
        var ajaxConfiguration = null;

        ajaxConfiguration = {
            url: baseUrl,
            crossdomain: true,
            headers: {
                "SOAPAction": soapAction,
            },
            type: httpDirective,
            dataType: httpDataType,
            data: soapRequest,
            processData: true,
            contentType: HTTP_POST_CONTENT_TYPE,
            complete: function (xmlHttpResponse, status) {
                completeEventHandler(xmlHttpResponse, status, callbackParams, additionalParams);
            }
        };

        $.ajax(ajaxConfiguration);
    };
    var loadPingListSuccessHandler = function (xmlHttpResponse, status, callbackParams, additionalParams) {
        if (status == AJAX_CALL_SUCCESS_STRING) {
            var targetList = {
                "pings": []

            };
            var pingListResult = $(xmlHttpResponse.responseXML).find(TN_TARGET_LIST_RESULT)[0];
            var pingList = getElement(pingListResult, pingNamespace, TN_PING_LIST, -1);
            $(pingList).each(function (index) {
                var pingerProfile = getFirstElement(this, pingNamespace, TN_PINGER_DESTINATION_PROFILE);
                var pingerSource = getFirstElementData(this, pingNamespace, TN_PINGER_SOURCE);
                var pingerDestination = getFirstElementData(this, pingNamespace, TN_PINGER_DESTINATION);
                var pingerImage = getFirstElementData(pingerProfile, pingNamespace, TN_PINGER_PROFILE_IMAGE);
                var pingerLastMessage = getFirstElementData(pingerProfile, pingNamespace, TN_PINGER_LAST_MESSAGE);

                var shouldIAdd = '' != pingerDestination && null !== pingerDestination;
                if (shouldIAdd) {
                    targetList.pings.push({
                        "Source": universalSource == pingerSource ? pingerSource : pingerDestination,
                        "Destination": universalSource == pingerSource ? pingerDestination : pingerSource,
                        "last_message": pingerLastMessage,
                        "ProfileImage": pingerImage
                    });
                }
            });
            updateMarkup(targetList, 'pingsList', 'newPing', 'contentHost', ['pings']);
        }
    };
    var postPingSuccessHandler = function (xmlHttpResponse, status, callbackParams, additionalParams) {
        var source = callbackParams[0];
        var target = callbackParams[1];
        var message = callbackParams[2];
        if (status == AJAX_CALL_SUCCESS_STRING) {
            var postResult = $(xmlHttpResponse.responseXML).find(TN_POST_PING_RESULT)[0];
            var serverPostResponse = $(postResult).text();
            if (serverPostResponse === 'true') {
                alert('Success !! message posted');
                additionalParams.reloadMessagesHandle(universalSource, target, 'pings', 'refreshRegion');
            } else {
                alert('something went wrong in server :(');
            }
        } else {
            alert('Not a great news :(');
        }
    };
    var listPingsSuccessHandler = function (xmlHttpResponse, status, callbackParams, additionalParams) {
        var left = callbackParams[0];
        var right = callbackParams[1];
        var targetRegion = callbackParams[2];
        var hostRegion = callbackParams[3];

        if (status == AJAX_CALL_SUCCESS_STRING) {
            var messagesResult = $(xmlHttpResponse.responseXML).find(TN_PINGS_RESULT)[0];
            var allMessages = messagesResult.getElementsByTagNameNS(pingNamespace, TN_PING_STRUCTURE);
            var displayMessage = {
                "pings": [],
                "profile": []
            };
            var profileIfNeverPinged = '';
            $(allMessages).each(function (index) {
                var shouldIAdd = 0 < $(this).children().length;
                if (shouldIAdd) {
                    var source = getFirstElementData(this, pingNamespace, TN_PING_SOURCE);
                    var timeSent = getLocalTimeFromUTC(getFirstElementData(this, pingNamespace, TN_PING_SENT_UTC));
                    var timeRecieved = getLocalTimeFromUTC(getFirstElementData(this, pingNamespace, TN_PING_RECIEVED_UTC));

                    displayMessage.pings.push({
                        "message_id": getFirstElementData(this, pingNamespace, TN_PING_ID),
                        "message": getFirstElementData(this, pingNamespace, TN_PING_TEXT),
                        "participant_direction": (source.trim() == left.trim()) ? "flat_layout_left" : "flat_layout_right",
                        "sent_time": isNaN(timeSent.getDate()) ? '*' : getTimeStampPrintFormat(timeSent),
                        "recieved_time": isNaN(timeRecieved.getDate()) ? '*' : getTimeStampPrintFormat(timeRecieved)
                    });

                    var destinedPingerProfile = getFirstElement(this, pingNamespace, TN_PING_DESTINED_PINGER_PROFILE);
                    var pingerProfile = getFirstElementData(destinedPingerProfile, pingNamespace, TN_PING_DESTINED_PINGER_IMAGE);
                    var destinationPinger = getFirstElementData(this, pingNamespace, TN_PING_DESTINED_PINGER);

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

            updateMarkup(displayMessage, targetRegion, 'newPing', hostRegion, ['pings']);

        } else {
            alert('Not a great news :(');
        }
    };
    return {
        loadPingList: function () {
            setUniversalSource();
            var soapRequest = SOAP_REQUEST_PING_LIST.replace('[Source]', universalSource);
            var template = null;
            var formattedMarkup = null;

            callServerAsync(baseUrl,
                                    SOAP_ACTION_PING_LIST,
                                    HTTP_POST_DIRECTIVE,
                                    HTTP_XML_DATA_DIRECTIVE,
                                    HTTP_POST_CONTENT_TYPE,
                                    soapRequest,
                                    loadPingListSuccessHandler,
                                    [],
                                    {});
        },
        startNewPing: function () {
            var template = null;
            var formattedMarkup = null;
            updateMarkup(null, 'newPing', 'newPing', 'contentHost', []);
        },
        startPinging: function () {
            var source = universalSource;
            var target = $(txtPingTarget).val();
            var message = $(txtPing).val();

            this.postPing(source, target, message);
        },
        listPings: function (left, right, targetRegion, hostRegion) {
            var source = left;
            var target = right;
            var soapRequest = SOAP_REQUEST_FETCH_PINGS.replace('[Destination]', target).replace('[Source]', source);
            callServerAsync(baseUrl,
                                    SOAP_ACTION_FETCH_PINGS,
                                    HTTP_POST_DIRECTIVE,
                                    HTTP_XML_DATA_DIRECTIVE,
                                    HTTP_POST_CONTENT_TYPE,
                                    soapRequest,
                                    listPingsSuccessHandler,
                                    [left, right, targetRegion, hostRegion],
                                    {});
        },
        postPing: function (source, target, message) {
            var now = new Date();
            var soapRequest = SOAP_REQUEST_POST_PING.replace('[Destination]', target)
                                        .replace('[Source]', source)
                                        .replace('[Message]', message)
                                        .replace('[SentUTC]', now.getUTCFullYear()
                                                                + "-" + padNumber(now.getUTCMonth() + 1)
                                                                + "-" + padNumber(now.getUTCDate())
                                                                + "T" + padNumber(now.getUTCHours())
                                                                + ":" + padNumber(now.getUTCMinutes())
                                                                + ":" + padNumber(now.getUTCSeconds())
                                                                + "." + padNumber(now.getUTCMilliseconds(), 2));
            callServerAsync(baseUrl,
                                    SOAP_ACTION_POST_PING,
                                    HTTP_POST_DIRECTIVE,
                                    HTTP_XML_DATA_DIRECTIVE,
                                    HTTP_POST_CONTENT_TYPE,
                                    soapRequest,
                                    postPingSuccessHandler,
                                    [source, target, message],
                                    { 'reloadMessagesHandle': this.listPings });
        },
        ping: function () {
            var target = $('#destinedPinger').text().trim();
            var message = $('#message_text').val();
            this.postPing(universalSource, target, message);
            var resetTextArea = setTimeout(function () { $('#message_text').val(''); }, 1000);
        }
    };
})();