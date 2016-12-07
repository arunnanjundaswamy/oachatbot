(function($){
    var API_URL = "http://blacksun.etrnl.us/api/bot";

    var api = (function(){
        return {
            getConversationList: function(){
                return $.getJSON(API_URL+'/vbbuserconversations');
            },
            getMessagesForConv: function(conversationId, date, msgId) {
                var query = ["conversationid=eq."+conversationId, "order=id.asc"];
                if(date){
                    date = moment(date).format('YYYY-MM-DD');
                    query.push('createddt=gte.'+date+'&createddt=lte.'+date+'T23:59:59')
                }
                if(msgId){
                    query.push('id=gte.'+msgId);
                }
                var url = API_URL + '/bbconversation?' +query.join('&');
                return $.getJSON(url);
            }
        };
    })();

    var attachHandlers = function(){
        var convList = $('#conv_list');
        var chatWindow = $('#chat_window');

        convList.on('click', 'a', function(){
            var convId = $(this).data('conv');
            var date = $(this).data('date');
            api.getMessagesForConv(convId, date).done(function(data){
                $('#chat_window').html(chatWindowTemplate({'messages':data}));
            });
            convList.find('a').removeClass('active');
            $(this).addClass('active');
        });

        chatWindow.on('click', '.linkable', function(){
            var link = $(this).data('link');
            if(link){
                window.getSelection().removeAllRanges();
                $('#clipboard_helper').val(window.location.href.split('#')[0] + '#' + link);
                var range = document.createRange();
                range.selectNode(document.getElementById('clipboard_helper'));
                window.getSelection().addRange(range);
                try {
                    // Now that we've selected the anchor text, execute the copy command
                    var successful = document.execCommand('copy');
                    var msg = successful ? 'successful' : 'unsuccessful';
                    console.log('Copy email command was ' + msg);
                    notify('Link copied to clipboard');
                } catch(err) {
                    console.log('Oops, unable to copy');
                }
            }
        });

        $(window).bind('hashchange', loadConvFromHash);
    };

    var notify = function(msg) {
        $('.notification').text(msg).addClass('show');
        setTimeout(function(){
            $('.notification').text(msg).removeClass('show');
        }, 2000);
    };

    var parseHash = function(){
        var p = window.location.href.split('#');
        if(p.length > 1){
            var hash = p[1];
            var parts = hash.split('.');
            var convId = parts[0];
            var msgId = parts[1];
            return { msgId: msgId, convId: convId };
        }
        return null;
    };

    var loadConvFromHash = function() {
        var hash = parseHash();
        if(hash){
            api.getMessagesForConv(hash.convId, null, hash.msgId).done(function(data){
                $('#chat_window').html(chatWindowTemplate({'messages':data}));
            });
        }
    };

    var convListTemplate = _.template($('#convListTemplate').html());
    var chatWindowTemplate = _.template($('#chatWindowTemplate').html());
    attachHandlers();
    api.getConversationList().done(function(data){
        $('#conv_list').html(convListTemplate({'conversations':data}));
    });
    loadConvFromHash();
})(jQuery);
