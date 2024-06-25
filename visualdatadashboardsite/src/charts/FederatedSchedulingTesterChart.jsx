import { useState, useEffect, useMemo } from "react";
//import { time_to_string_HHmmss } from "../auxiliary/time/timehelper";
//import LineChartStandard from "./LineChartStandard";
import useFetch from "../endpoints/api/useFetch";
import { HTTPResponseParser, api_full_path, api_full_path_with_query, api_post, get_api } from "../endpoints/api/api_helper";
import { APIEndpoints } from "../site_config.json";



const FederatedSchedulingTesterChart = ({ title, refreshTriggered, updateRefreshStatus }) => {

    const [fileSelectInput, setFileSelectInput] = useState("");
    const [processorCountSelectInput, setProcessorCountSelectInput] = useState(0);
    const [downloadTaskSet, setDownloadTaskSet] = useState(false);
    const [downloadSchedule, setDownloadSchedule] = useState(true);
    const [testOutput, setTestOutput] = useState(null);

    const { data: tasksetOptions, refreshData: refreshTasksetOptions, isPending, error } = useFetch(api_full_path(APIEndpoints.LinuxSystemTestServer.url, get_api(APIEndpoints.LinuxSystemTestServer, "get_federated_scheduling_tasksets").path));

    useEffect(() => {
        /*
        if (isRefreshTriggered) {
            refreshTasksetOptions();
            updateRefreshStatus(false, true);
        }
        else
            updateRefreshStatus(false, false);
        */
        refreshTasksetOptions();
        setFileSelectInput("TaskSetTest20240223");
        setProcessorCountSelectInput(6);
    }, [refreshTriggered, refreshTasksetOptions]);


    const runSchedulingTest = async () => {
        setTestOutput("");
        let full_path = api_full_path_with_query(APIEndpoints.LinuxSystemTestServer.url, get_api(APIEndpoints.LinuxSystemTestServer, "federated_scheduling_test").path, `TaskSetID=${fileSelectInput}&ProcessorCount=${processorCountSelectInput}`);
        let response_obj = await api_post(full_path, {});
        setTestOutput(response_obj ? response_obj.message : "ERROR");

        full_path = api_full_path_with_query(APIEndpoints.LinuxSystemTestServer.url, get_api(APIEndpoints.LinuxSystemTestServer, "federated_scheduling_download").path, `TaskSetID=${fileSelectInput}&ProcessorCount=${processorCountSelectInput}`);
        response_obj = await api_post(full_path, {
            download_task_set: downloadTaskSet,
            download_schedule: downloadSchedule
        }, HTTPResponseParser.blob);

        const aElement = document.createElement('a');
        aElement.setAttribute('download', fileSelectInput + ".zip");
        const href = URL.createObjectURL(response_obj);
        aElement.href = href;
        aElement.setAttribute('target', '_blank');
        aElement.click();
        URL.revokeObjectURL(href);
        //return response_obj;
    };

    
    /*
    const files_config = useMemo(() => {
        return [
            { label: "TaskSetTest20240216", value: "TaskSetTest20240216"},
            { label: "TaskSetTest20240222", value: "TaskSetTest20240222" },
            { label: "TaskSetTest20240223", value: "TaskSetTest20240223" },
            { label: "TaskSetTest20240304", value: "TaskSetTest20240304" }
        ];
    }, []);
    */
    
    const processors_config = useMemo(() => {
        return [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
    }, []);
    


    return (
        <div className="container full-height">
            <div className="container full-height scroll-control-y">
                <div style={{ alignContent: "center" }}>
                    <span>Task Set: </span>
                    <select className="query-field-input select" value={fileSelectInput} onChange={(e) => { setFileSelectInput(e.target.value); }}>
                        {/*<option disabled value=""> -- select an option -- </option>*/}
                        {tasksetOptions && !isPending && tasksetOptions.task_sets.map((opt) => <option key={opt} value={opt}>{opt}</option>)}
                    </select>
                    <span>    </span>
                    <span># of Processor: </span>
                    <select className="query-field-input select" value={processorCountSelectInput} onChange={(e) => { setProcessorCountSelectInput(e.target.value); }}>
                        {/*<option disabled value=""> -- select an option -- </option>*/}
                        {processors_config.map((opt) => <option key={opt} value={opt}>{opt}</option>)}
                    </select>
                    <span>    </span>
                    <span>Download: </span>
                    <input type="checkbox" id="tasksetjsonpdf" name="tasksetjsonpdf" value="tasksetjsonpdf" onChange={(e) => { setDownloadTaskSet(e.target.checked); }} />
                    <label>task-set.*</label>
                    <input type="checkbox" id="schedulecsv" name="schedulecsv" value="schedulecsv" defaultChecked="true" onChange={(e) => { setDownloadSchedule(e.target.checked); }} />
                    <label>schedule.csv</label>
                    <span>    </span>

                    <button className="query-interface-input button" onClick={runSchedulingTest}>Run Scheduling Test</button>
                </div>
                <div style={{ whiteSpace: "pre-line" }}>{testOutput}</div>
            </div>
            
        </div>
    );
};

export default FederatedSchedulingTesterChart;
