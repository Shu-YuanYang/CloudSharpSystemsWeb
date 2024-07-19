import { useState, useEffect, useMemo } from "react";
//import { time_to_string_HHmmss } from "../auxiliary/time/timehelper";
//import LineChartStandard from "./LineChartStandard";
import useFetch from "../endpoints/api/useFetch";
import { api_full_path, api_post, get_api } from "../endpoints/api/api_helper";
import { APIEndpoints } from "../site_config.json";



const SGDVariableSelectionTesterChart = ({ title, refreshTriggered, updateRefreshStatus }) => {

    const [fileSelectInput, setFileSelectInput] = useState("");
    const [deviceSelectInput, setDeviceSelectInput] = useState("");
    const [chunkSize, setChunkSize] = useState(10000);
    const [batchSize, setBatchSize] = useState(200);
    const [learningRate, setLearningRate] = useState(0.02);
    const [epochs, setEpochs] = useState(30);
    const [variableSelectionDoFThreshold, setVariableSelectionDoFThreshold] = useState(100);
    const [testOutput, setTestOutput] = useState("");

    const [isTesting, setIsTesting] = useState(false);

    const { data: datasetOptions, refreshData: refreshDatasetOptions, isPending, error } = useFetch(api_full_path(APIEndpoints.OKISGDServer.url, get_api(APIEndpoints.OKISGDServer, "get_sgd_datasets").path));

    useEffect(() => {
        /*
        if (refreshTriggered) {
            refreshDatasetOptions();
            updateRefreshStatus(false, true);
        }
        else
            updateRefreshStatus(false, false);
        */

        refreshDatasetOptions();
        setFileSelectInput("sgemm_product_standardized.csv");
        setDeviceSelectInput("cpu");
        setChunkSize(10000);
        setBatchSize(200);
        setLearningRate(0.02);
        setEpochs(30);
        setVariableSelectionDoFThreshold(100);
        setTestOutput("");
    }, [refreshTriggered, refreshDatasetOptions]);


    const runSGDVariableSelectionTest = async () => {
        setIsTesting(true);
        setTestOutput("Model is training. Please wait...\n(Contact the developer is the wait is more than 60 seconds).\nApologies for the lack of interativeness.");
        let full_path = api_full_path(APIEndpoints.OKISGDServer.url, get_api(APIEndpoints.OKISGDServer, "sgd_variable_selection_test").path);
        let response_obj = await api_post(full_path, {
            dataset_file: fileSelectInput,
            device: deviceSelectInput,
            chunk_size: chunkSize,
            batch_size: batchSize,
            learning_rate: learningRate,
            epochs: epochs,
            dof_threshold: variableSelectionDoFThreshold
        });
        
        setTestOutput(response_obj ? response_obj.message : "ERROR");
        setIsTesting(false);
    };


    const device_config = useMemo(() => {
        return ["cpu", "gpu"];
    }, []);

    return (
        <div className="container full-height">
            <div className="container full-height scroll-control-y">
                <div className="container align-center">
                    <span>Dataset: </span>
                    <select className="query-field-input select" value={fileSelectInput} onChange={(e) => { setFileSelectInput(e.target.value); }}>
                        {/*<option disabled value=""> -- select an option -- </option>*/}
                        {datasetOptions && !isPending && datasetOptions.datasets.map((opt) => <option key={opt} value={opt}>{opt}</option>)}
                    </select>
                    <span>    </span>
                    <span>Device: </span>
                    <select className="query-field-input select" value={deviceSelectInput} onChange={(e) => { setDeviceSelectInput(e.target.value); }}>
                        {/*<option disabled value=""> -- select an option -- </option>*/}
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
                </div>
                <div style={{ whiteSpace: "pre-line" }}>{
                    (testOutput === "" && datasetOptions && !isPending) ?
                    datasetOptions.user_guide : testOutput
                }</div>
            </div>

        </div>
    );
};

export default SGDVariableSelectionTesterChart;
