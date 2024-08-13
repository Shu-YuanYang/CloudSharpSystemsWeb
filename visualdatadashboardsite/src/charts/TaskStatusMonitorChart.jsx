import TablePartitions from "../tables/TablePartitions";
import LoadingComponentWrapper from "../auxiliary/wrappers/LoadingComponentWrapper";

import { useEffect, useMemo } from "react";
import { api_full_path_with_query, get_api } from "../endpoints/api/api_helper";
import useFetch from "../endpoints/api/useFetch";
import { APIEndpoints } from "../site_config.json";


const TaskStatusMonitorChart = ({ title, refreshTriggered, updateRefreshStatus }) => {
    
    const { data: statusData, refreshData: refreshStatusData, isPending: isStatusDataPending, error: statusDataFetchError } = useFetch(api_full_path_with_query(APIEndpoints.CloudSharpMicroService.url, get_api(APIEndpoints.CloudSharpMicroService, "get_task_status_statistics").path, "app_id=CloudSharpTaskManager"));

    const standardizedData = useMemo(() => {
        if (!statusData) return [];
        const data = statusData.map(item => ({ key: `STATUS: ${item.status}`, dataList: item.statistics }))
        return data;
    }, [statusData]);

    useEffect(() => {
        refreshStatusData();
    }, [refreshTriggered, refreshStatusData]);

    return (
        <div className="container horizontal-centering full-height">
            <div className="c95 container-full-height">
                <br />
                {!isStatusDataPending && standardizedData && <TablePartitions title={title} rawData={standardizedData} columnConfig={{}} refreshData={refreshStatusData} />}
                {isStatusDataPending && <LoadingComponentWrapper />}
            </div>
        </div>
    );
};

export default TaskStatusMonitorChart;
