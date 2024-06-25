import { useState, useEffect, useCallback } from "react";


const useAuthorizedFetch = (url, auth_header = null, api_method = "GET", api_body = null) => {

    const [data, setData] = useState(null);
    const [isRefreshing, setIsRefreshing] = useState(true);
    const [isPending, setIsPending] = useState(true);
    const [error, setError] = useState(null);

    const refreshData = useCallback(() => {
        setIsRefreshing(true);
        setIsPending(true);
        setError(null);
    }, []);


    const fetchFunction = useCallback((abortCont) => {

        if (!auth_header) {
            setData(null);
            setIsRefreshing(false);
            setIsPending(false);
            setError(null);
            return; // skip server request if authorization header is empty
        }

        fetch(url, {
            signal: abortCont.signal,
            method: api_method,
            headers: {
                'Authorization': `Bearer ${auth_header}`,
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: api_body ? JSON.stringify(api_body) : null
        }).then((res) => {
            if (!res.ok) { throw Error("could not fetch the data on that resource!"); }
            return res.json();
        }).then((data) => {
            console.log(data);
            setData(data);
            setIsRefreshing(false);
            setIsPending(false);
            setError(null);
        }).catch((err) => {
            if (err.name === "AbortError") {
                console.log("fetch aborted");
            } else {
                setIsPending(false);
                setError(err.message);
            }
        });

    }, [url, api_method, auth_header, api_body]);


    // Upon changes to input, refresh
    useEffect(refreshData, [url, api_method, auth_header, api_body, refreshData]);


    // Upon refresh, send request to server:
    useEffect(() => {

        const abortCont = new AbortController();
        const func = () => abortCont.abort();

        if (!isRefreshing) return func; // skip server request refresh command is set to false
        fetchFunction(abortCont);

        return func;
    }, [isRefreshing, fetchFunction]);

    //useEffect(refreshData, [api_method, api_body, refreshData]);

    return { data, refreshData, isPending, error };
}

export default useAuthorizedFetch;

