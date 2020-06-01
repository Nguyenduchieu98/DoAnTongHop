function lstImpressions(data) {
    dataLayer.push({
        'event' : 'EEimpression',
        'ecommerce': {
            'currencyCode': 'USD',
            'impressions': data
        }
    });
}

function lstDetail(product) {
    dataLayer.push({
        'event' : 'EEDetail',
        'ecommerce': {
            'detail': {
                'actionField': {'list': ''},
                'products': product
            }
        }
    });
}

function lstAddToCart(product) {
    dataLayer.push({
        'event': 'EEaddToCart',
        'ecommerce': {
            'currencyCode': 'USD',
            'add': {
                'products': product
            }
        }
    });
}

function lstOnCheckout(product, option) {
    dataLayer.push({
        'event': 'EEcheckout',
        'ecommerce': {
            'checkout': {
                'actionField': {'step': 1, 'option': option},
                'products': product
            }
        },
        'eventCallback': function() {
            
        }
    });
}

function lstPurchase(products, purchase) {
    dataLayer.push({
        'event' : 'EEtransaction',
        'ecommerce': {
            'purchase': {
                'actionField': purchase,
                'products': products
            }
        }
    });
}

//Pixel
// function lstPixelSearch(data) {
//     data.forEach(function(value, key){
//         fbq('track', 'Search', value );
//     });
// }

function lstListingCriteo(data) {
    if (typeof data !== 'undefined' && data.length > 0) {
        window.criteo_q.push(
            { event: "setAccount", account: cri_account },
            { event: "setEmail", email: cri_email },
            { event: "setSiteType", type: cri_device_type },
            { event: "viewList", item: data }
        );
    }
}

//Pixel
function lstPixelSearch(data) {
    fbq('track', 'Search', {
        content_type: 'hotel',
        checkin_date: getCheckinDateFromUrl(),
        checkout_date: getCheckinDateFromUrl,
        content_ids: data,
        city: '',
        region: '',
        country: '',
        num_adults: getNumberGuestFromUrl(),
        num_children: 0
    });
}

function getCheckinDateFromUrl() {
    var urlParameter = getAllUrlParams(window.location.href);
    return urlParameter.checkin;
}

function getCheckoutDateFromUrl() {
    var urlParameter = getAllUrlParams(window.location.href);
    return urlParameter.checkin;
}

function getNumberGuestFromUrl() {
    var urlParameter = getAllUrlParams(window.location.href);
    return urlParameter.guests;
}

function getAllUrlParams(url) {

    // get query string from url (optional) or window
    var queryString = url ? url.split('?')[1] : window.location.search.slice(1);

    // we'll store the parameters here
    var obj = {};

    // if query string exists
    if (queryString) {

        // stuff after # is not part of query string, so get rid of it
        queryString = queryString.split('#')[0];

        // split our query string into its component parts
        var arr = queryString.split('&');

        for (var i = 0; i < arr.length; i++) {
            // separate the keys and the values
            var a = arr[i].split('=');

            // set parameter name and value (use 'true' if empty)
            var paramName = a[0];
            var paramValue = typeof (a[1]) === 'undefined' ? true : a[1];

            // (optional) keep case consistent
            paramName = paramName.toLowerCase();
            if (typeof paramValue === 'string') {
                paramValue = paramValue.toLowerCase();
            }
            if (paramName.match(/\[(\d+)?\]$/)) {

                // create key if it doesn't exist
                var key = paramName.replace(/\[(\d+)?\]/, '');
                if (!obj[key]) obj[key] = [];

                // if it's an indexed array e.g. colors[2]
                if (paramName.match(/\[\d+\]$/)) {

                    var index = /\[(\d+)\]/.exec(paramName)[1];
                    obj[key][index] = paramValue;
                } else {
                    // otherwise add the value to the end of the array
                    obj[key].push(paramValue);
                }
            } else {
                // we're dealing with a string
                if (!obj[paramName]) {
                    // if it doesn't exist, create property
                    obj[paramName] = paramValue;
                } else if (obj[paramName] && typeof obj[paramName] === 'string'){
                    obj[paramName] = [obj[paramName]];
                    obj[paramName].push(paramValue);
                } else {
                    // otherwise add the property
                    obj[paramName].push(paramValue);
                }
            }
        }
    }

    return obj;
}