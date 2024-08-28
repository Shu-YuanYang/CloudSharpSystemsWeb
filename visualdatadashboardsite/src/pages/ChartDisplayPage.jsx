import "../styles/DataDetail.css";
import { useState, useMemo } from "react";
import DynamicComponentWrapper from "../auxiliary/wrappers/DynamicComponentWrapper";
import { ErrorBoundary } from "../auxiliary/wrappers/ErrorBoundaryWrapper";
import { useParams, useLocation } from "react-router-dom";
import { usePageTitle } from "./page_title_converter";

function useQuery() {
    const { search } = useLocation();
    return useMemo(() => new URLSearchParams(search), [search]);
}

const ChartDisplayPage = ({ title }) => {

    const { chart } = useParams();
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


    // Use function to load data:

    const [refreshTriggered, setRefreshTriggered] = useState(0);
    const [isRefreshing, setIsRefreshing] = useState(false);

    
    const update_refresh_status = (is_refresh_triggered, is_refreshing) => {
        setRefreshTriggered((count) => (count + 1) % 10);
        setIsRefreshing(true);
        /*const timeout = */setTimeout(() => { setIsRefreshing(false); }, 3000);
    }

    




    return (
        <div className="board main">
            <div className="container horizontal-centering full-height">
                <div className="c95 container-full-height">
                    <div className="page title">
                        <span>{page_title}</span>
                    </div>
                    <br />
                    <ErrorBoundary fallback={<h4>Unable to display chart. An error occurred!</h4>}>
                        {//!isRefreshing &&
                            <div className="card-editor">
                                <div className="card-button">
                                    <button className="button-small" onClick={() => { update_refresh_status(true, true); }} disabled={isRefreshing}>Refresh</button>
                                </div>
                            </div>
                        }
                        <div className="r85" style={{ height: "85%" }}>
                            <DynamicComponentWrapper directory={"charts"} component_name={chart} title={page_title} refreshTriggered={refreshTriggered} />
                        </div>
                    </ErrorBoundary>
                </div>
            </div>
        </div>
    );
};


export default ChartDisplayPage;