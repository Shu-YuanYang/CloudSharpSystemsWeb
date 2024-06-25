import TableWithSummary from "../tables/TableWithSummary";
import LoadingComponentWrapper from "../auxiliary/wrappers/LoadingComponentWrapper";

import { useMemo } from "react";
import { api_full_path, get_api } from "../endpoints/api/api_helper";
import useFetch from "../endpoints/api/useFetch";
import { APIEndpoints } from "../site_config.json";



const DashboardTableSimpleView = ({ title, dataSourceAPIEndpoint, dataSourceAPIName }) => {

    const data_source_api = useMemo(() => {
        const api = get_api(APIEndpoints[dataSourceAPIEndpoint], dataSourceAPIName);
        return api;
    }, [dataSourceAPIEndpoint, dataSourceAPIName]);

    const { data, refreshData, isPending, error } = useFetch(api_full_path(APIEndpoints[dataSourceAPIEndpoint].url, data_source_api.path), data_source_api.method, data_source_api.default_body);

    


    // const [data...] = useFetch(dataSourceUrl);
    //const isPending = false;
    //const [data, setData] = useState(makeSortablePageData());
    //const refreshData = () => { console.log("refresh"); };

    return (
        <div className="container horizontal-centering full-height">
            <div className="c95 container-full-height">
                <div className="page title">
                    <span>{title}</span>
                </div>
                <br />
                {/*<div className="container horizontal-centering">
                    <h1>{title}</h1>
                </div>*/}
                {!isPending && data && <TableWithSummary title={title} rawData={data} columnConfig={{}} refreshData={refreshData} />}
                {isPending && <LoadingComponentWrapper />}
            </div>
        </div>
    );
};

export default DashboardTableSimpleView;
