import { useState, useEffect, useCallback } from "react";


const useFetch = (url, api_method = "GET", api_body = null) => {

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
        //console.log(url);

        fetch(url, {
            signal: abortCont.signal,
            method: api_method,
            headers: {
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
    }, [url, api_method, api_body]);


    // if url, method, or body changes, refresh the fetch
    useEffect(refreshData, [url, api_method, api_body, refreshData]);


    useEffect(() => {
        const abortCont = new AbortController();
        const func = () => abortCont.abort();

        if (!isRefreshing) return func;
        fetchFunction(abortCont);

        return func;
    }, [isRefreshing, fetchFunction]);


    return { data, refreshData, isPending, error };
}

export default useFetch;

