

export function get_api(endpoint, api_name) {
    return endpoint.api[api_name];
}


export function api_full_path(url, subpath) {
    if (subpath === "") return url;
    if (url[url.length - 1] !== "/" && subpath[0] !== "/") url += "/";
    return url + subpath;
}

export function api_full_path_with_query(url, subpath, queryStr) {
    let api_path = api_full_path(url, subpath);
    if (!queryStr || queryStr == "") return api_path;
    const queryHead = (api_path.includes("?")) ? "&" : "?";
    if (queryStr[0] != "?" && queryStr[0] != "&") queryStr = queryHead + queryStr;
    api_path = encodeURI(api_path + queryStr);
    //console.log(api_path);
    return api_path;
}


export const HTTPResponseParser = {
    json: async (response) => await response.json(),
    blob: async (response) => await response.blob()
};

export const api_post = async (endpoint, body, parse_response = HTTPResponseParser.json) => {
    let response_data;
    //try {
    const response = await fetch(endpoint, {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(body)
    });
    //response_data = await response.json();
    response_data = await parse_response(response);
    if (!response.ok) throw new Error(response_data.Message);
    console.log(response_data);
    //} catch (error) {
    //    response_data = error;
    //    console.error('Unable to finish post request.', error)
    //}
    return response_data;
};



export const api_authorized_get = async (endpoint, authorization, parse_response = HTTPResponseParser.json) => {
    let response_data;

    const response = await fetch(endpoint, {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${authorization}`,
            'Accept': 'application/json'
        }
    });
    //console.log("response.ok: ", response.ok);
    response_data = await parse_response(response);
    if (!response.ok) throw new Error(response_data.Message);
    console.log(response_data);

    return response_data;
};


export const api_authorized_post = async (endpoint, authorization, body, parse_response = HTTPResponseParser.json) => {
    let response_data;
    
    const response = await fetch(endpoint, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${authorization}`,
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(body)
    });
    //console.log("response.ok: ", response.ok);
    response_data = await parse_response(response);
    if (!response.ok) throw new Error(response_data.Message);
    
    console.log(response_data);
    
    return response_data;
};


export const api_authorized_post_form_data = async (endpoint, authorization, body, parse_response = HTTPResponseParser.json) => {
    let response_data;

    const response = await fetch(endpoint, {
        method: 'POST',
        headers: {
            'Authorization': `Bearer ${authorization}`
        },
        body: body
    });
    //console.log("response.ok: ", response.ok);
    response_data = await parse_response(response);
    if (!response.ok) throw new Error(response_data.Message);
    console.log(response_data);

    return response_data;
};