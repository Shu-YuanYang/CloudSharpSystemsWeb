import DashboardItemMenuHeader from "./DashboardItemMenuHeader";
//import reactLogo from '../assets/react.svg';
//import LineChartStandard from "../charts/LineChartStandard";
import { useState, useEffect } from "react";
//import { time_to_string_HHmmss, string_expression_to_time_obj } from "../auxiliary/time/timehelper";
//import DBLatencyMonitorChart from "../charts/DBLatencyMonitorChart";
import DynamicComponentWrapper from "../auxiliary/wrappers/DynamicComponentWrapper";
import { ErrorBoundary } from "../auxiliary/wrappers/ErrorBoundaryWrapper";


const DashboardMonitor = ({ title, currentComponentData }) => { 
    
    // Use function to load data:

    const [refreshTriggered, setRefreshTriggered] = useState(0);
    const [isRefreshing, setIsRefreshing] = useState(false);
    
    /*
    useEffect(() => {
        setIsRefreshing(false);  // reset rendering cycle upon component change
    }, [currentComponentData]);
    */
    
    const update_refresh_status = (is_refresh_triggered, is_refreshing) => {
        setRefreshTriggered((count) => (count + 1) % 10); 
        setIsRefreshing(true);
        /*const timeout = */setTimeout(() => { setIsRefreshing(false); }, 3000);
    }
    
    



    return (
        <div className="container full-height">
            <div className="dashboard monitor">
                <DashboardItemMenuHeader title={title}>
                    { currentComponentData && 
                        <div className="card-editor">
                            <div className="card-button">
                                <button className="button-small" onClick={() => { update_refresh_status(true, true); }} disabled={isRefreshing}>Refresh</button>
                            </div>
                        </div>
                    }
                </DashboardItemMenuHeader>
                {currentComponentData && (
                    <div className="container content-height">
                        <ErrorBoundary fallback={<h4>Unable to display chart. An error occurred!</h4>}>
                            <DynamicComponentWrapper directory={"charts"} component_name={currentComponentData.ROUTE} title={currentComponentData.DISPLAY_NAME} refreshTriggered={refreshTriggered} updateRefreshStatus={update_refresh_status} />
                        </ErrorBoundary>
                    </div>
                )}
                
                {/*<DBLatencyMonitorChart title="App DB Query Latencies" isRefreshing={isRefreshing} completeRefreshing={() => { setIsRefreshing(false); }} />*/}
            </div>
        </div>
    );
};


export default DashboardMonitor;