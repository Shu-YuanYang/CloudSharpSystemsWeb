import TableWithSummary from "../tables/TableWithSummary";
import LoadingComponentWrapper from "../auxiliary/wrappers/LoadingComponentWrapper";

import { useMemo, useState } from "react";
//import { QueryListBuilder } from "../endpoints/api/query_helper";
import { api_full_path_with_query, get_api } from "../endpoints/api/api_helper";
import useAuthorizedFetch from "../endpoints/api/useAuthorizedFetch";
import useFetch from "../endpoints/api/useFetch";
import { useContext } from 'react';
import IdentityContext from "../auxiliary/wrappers/IdentityContext";
import { APIEndpoints, DataTableViewConfig } from "../site_config.json";
import TableQueryInterface from "../tables/TableQueryInterface";


const DashboardTableQueryableView = ({ title, dataSourceAPIEndpoint, dataSourceAPIName }) => {

    const userIdentity = useContext(IdentityContext);

    // Load queryable fields config:
    const queryConfig = DataTableViewConfig[title].QuerySelectorAPI;
    
    const query_config_source_api = useMemo(() => {
        const api = get_api(APIEndpoints[queryConfig.endpoint], queryConfig.api);
        return api;
    }, [queryConfig]);

    const { data: queryableConfigData, refreshData: refreshQueryableConfigData, isPending: isQueryableConfigDataPending, error: queryableConfigDataError } = useFetch(api_full_path_with_query(APIEndpoints[queryConfig.endpoint].url, query_config_source_api.path, queryConfig.url_query), query_config_source_api.method, query_config_source_api.default_body); 

    
    // Load table data:
    const data_source_api = useMemo(() => {
        const api = get_api(APIEndpoints[dataSourceAPIEndpoint], dataSourceAPIName);
        return api;
    }, [dataSourceAPIEndpoint, dataSourceAPIName]);

    const [requestBody, setRequestBody] = useState(data_source_api.default_body);
    const { data, refreshData, isPending, error } = useAuthorizedFetch(api_full_path_with_query(APIEndpoints[dataSourceAPIEndpoint].url, data_source_api.path, ""), userIdentity ? userIdentity.session_id : null, data_source_api.method, requestBody);




    const submit_query = (queryList) => {
        setRequestBody(queryList);
    };

    return (
        <div className="container horizontal-centering full-height scroll-control-y">
            <div className="c95 container-full-height">
                <div className="page title">
                    <span>{title}</span>
                </div>
                <br />
                {/*<div className="container horizontal-centering">
                    
                    <h1>{title}</h1>
                </div>*/}
                <TableQueryInterface queryFunction={submit_query} queryableConfigData={queryableConfigData} isQueryPending={isPending} />
                {/*<button onClick={submit_query}>Query</button>*/}
                {!isPending && data && <TableWithSummary title={title} rawData={data} columnConfig={{}} refreshData={refreshData} />}
                {isPending && <LoadingComponentWrapper />}
            </div>
        </div>
    );
};

export default DashboardTableQueryableView;
