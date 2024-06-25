//import DashboardTableSimpleView from "../dashboard_core/DashboardTableSimpleView";
import "../styles/DataDetail.css";

import { useMemo } from "react";
import { useParams, useLocation } from "react-router-dom";
import DynamicComponentWrapper from "../auxiliary/wrappers/DynamicComponentWrapper";
import { usePageTitle } from "./page_title_converter";

//import { api_full_path, get_api } from "../endpoints/api/api_helper";
//import { APIEndpoints } from "../site_config.json";





function useQuery() {
    const { search } = useLocation();
    return useMemo(() => new URLSearchParams(search), [search]);
}

const DataDetailPage = ({ title }) => {

    
    const { componentName, dataSourceAPIEndpoint, dataSourceAPIName } = useParams();
    const query = useQuery();

    /*const data_source_url = useMemo(() => {
        const api = get_api(APIEndpoints[dataSourceAPIEndpoint], dataSourceAPIPath);
        return api_full_path(APIEndpoints[dataSourceAPIEndpoint].url, api.path, api.default_body);
    }, [dataSourceAPIEndpoint, dataSourceAPI]);*/

    const page_title = useMemo(() => {
        const param_title = query.get("title");
        return param_title ? param_title : title;
    }, [title, query]);

    usePageTitle(page_title);

    return (
        <div className="board main">
            <DynamicComponentWrapper directory={"dashboard_core"} component_name={componentName} title={page_title} dataSourceAPIEndpoint={dataSourceAPIEndpoint} dataSourceAPIName={dataSourceAPIName} />
            {/*<DashboardTableSimpleView title={title} dataSourceUrl={dataSourceUrl} />*/}
        </div>
    );
};

export default DataDetailPage;