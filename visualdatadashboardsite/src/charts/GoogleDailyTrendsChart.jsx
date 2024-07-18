import { useState, useEffect, useMemo } from "react";
//import { time_to_string_HHmmss } from "../auxiliary/time/timehelper";
//import LineChartStandard from "./LineChartStandard";
import useFetch from "../endpoints/api/useFetch";
import { api_full_path, api_post, get_api } from "../endpoints/api/api_helper";
import { APIEndpoints } from "../site_config.json";
import TableList from "../tables/TableList";


const GoogleDailyTrendsChart = ({ title, refreshTriggered, updateRefreshStatus = null }) => {

    const [formattedTrends, setFormattedTrends] = useState([
        {
            title: "Bob Menendez", source: "ABC News", link: "https://abcnews.go.com/US/sen-bob-menendez-federal-corruption-trial-verdict/story?id=111295557",
            expansion_action: { text: "Relevant Searches" },
            subLists: [
                { title: "Sen. Bob Menendez found guilty on all counts, including acting as ...", source: "ABC News", link: "https://abcnews.go.com/US/sen-bob-menendez-federal-corruption-trial-verdict/story?id=111295557" },
                { title: "Democratic Sen. Bob Menendez found guilty on all counts in ...", source: "NBC News", link: "https://www.nbcnews.com/politics/congress/democratic-sen-bob-menendez-found-guilty-counts-corruption-charges-rcna159955" },
                { title: "Senator Bob Menendez found guilty in bribery scheme", source: "BBC News", link: "https://www.bbc.com/news/articles/cqe6m14drgjo" }
            ]
        },
        {
            title: "Joe Bryant", source: "La Salle University Athletics", link: "https://goexplorers.com/news/2024/7/16/la-salle-mourns-the-passing-of-former-mens-basketball-star-joe-bryant.aspx",
            expansion_action: { text: "Relevant Searches" },
            subLists: [
                { title: "La Salle Mourns the Passing of Former Men&#39;s Basketball Star Joe ...", source: "La Salle University Athletics", link: "https://goexplorers.com/news/2024/7/16/la-salle-mourns-the-passing-of-former-mens-basketball-star-joe-bryant.aspx" },
                { title: "Democratic Sen. Bob Menendez found guilty on all counts in ...", source: "NBC News", link: "https://www.nbcnews.com/politics/congress/democratic-sen-bob-menendez-found-guilty-counts-corruption-charges-rcna159955" },
            ]
        },
        {
            title: "Elon Musk", source: "CNNMoney", link: "https://www.cnn.com/2024/07/16/business/elon-musk-spacex-x-texas/index.html",
            expansion_action: { text: "Relevant Searches" },
            subLists: [
                { title: "Elon Musk says he&#39;s moving SpaceX and X out of California", source: "CNNMoney", link: "https://www.cnn.com/2024/07/16/business/elon-musk-spacex-x-texas/index.html" },
                { title: "Elon Musk says he&#39;s moving SpaceX, X headquarters from ...", source: "ABC News", link: "https://abcnews.go.com/Business/wireStory/elon-musk-moving-spacex-headquarters-california-texas-112002570" },
            ]
        }
    ]);

    const [isLoading, setIsLoading] = useState(false);

    /*
    const { data: datasetOptions, refreshData: refreshDatasetOptions, isPending, error } = useFetch(api_full_path(APIEndpoints.OKISGDServer.url, get_api(APIEndpoints.OKISGDServer, "get_sgd_datasets").path));

    useEffect(() => {
        refreshDatasetOptions();
        setFileSelectInput("sgemm_product_standardized.csv");
        setDeviceSelectInput("cpu");
        setChunkSize(10000);
        setBatchSize(200);
        setLearningRate(0.02);
        setEpochs(30);
        setVariableSelectionDoFThreshold(100);
        setTestOutput("");
    }, []);
    */
    

    return (
        <div className="container full-height">
            <div className="container full-height scroll-control-y">
                <div style={{ alignContent: "center" }}>
                    <TableList contentList={formattedTrends} refreshDisplay={() => { } } />
                    {/*
                    <span>Dataset: </span>
                    <select className="query-field-input select" value={fileSelectInput} onChange={(e) => { setFileSelectInput(e.target.value); }}>
                        {datasetOptions && !isPending && datasetOptions.datasets.map((opt) => <option key={opt} value={opt}>{opt}</option>)}
                    </select>
                    <span>    </span>
                    <span>Device: </span>
                    <select className="query-field-input select" value={deviceSelectInput} onChange={(e) => { setDeviceSelectInput(e.target.value); }}>
                        {device_config.map((opt) => <option key={opt} value={opt}>{opt}</option>)}
                    </select>
                    
                    <span>    </span>
                    <span>Chunk size: </span>
                    <input className="query-field-input text-box" id="chunksize" name="chunksize" value={chunkSize} onChange={(e) => { setChunkSize(e.target.value); }} />
                    <span>    </span>
                    <span>Batch size: </span>
                    <input className="query-field-input text-box" id="batchsize" name="batchsize" value={batchSize} onChange={(e) => { setBatchSize(e.target.value); }} />
                    <span>    </span>
                    <span>Learning rate:</span>
                    <input className="query-field-input text-box" id="learningrate" name="learningrate" value={learningRate} onChange={(e) => { setLearningRate(e.target.value); }} />
                    <span>    </span>
                    <span>Epochs:</span>
                    <input className="query-field-input text-box" id="epochs" name="epochs" value={epochs} onChange={(e) => { setEpochs(e.target.value); }} />
                    <span>    </span>
                    <span>Degree of freedom threshold:</span>
                    <input className="query-field-input text-box" id="dofthreshold" name="dofthreshold" value={variableSelectionDoFThreshold} onChange={(e) => { setVariableSelectionDoFThreshold(e.target.value); }} />
                    <span>    </span>

                    {
                        !isTesting && 
                        <button className="query-interface-input button" onClick={runSGDVariableSelectionTest}>Run SGD Test</button>
                    }
                    */
                    }
                </div>
                <div style={{ whiteSpace: "pre-line" }}>{
                    isLoading? "..." : ""
                }</div>
            </div>

        </div>
    );
};

export default GoogleDailyTrendsChart;
