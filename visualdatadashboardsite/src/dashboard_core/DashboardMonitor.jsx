import DashboardItemMenuHeader from "./DashboardItemMenuHeader";
//import reactLogo from '../assets/react.svg';
//import LineChartStandard from "../charts/LineChartStandard";
import { useState, useEffect } from "react";
//import { time_to_string_HHmmss, string_expression_to_time_obj } from "../auxiliary/time/timehelper";
//import DBLatencyMonitorChart from "../charts/DBLatencyMonitorChart";
import DynamicComponentWrapper from "../auxiliary/wrappers/DynamicComponentWrapper";
import { ErrorBoundary } from "../auxiliary/wrappers/ErrorBoundaryWrapper";
import { useContext } from 'react';
import HomeMonitorRefreshContext from "../auxiliary/wrappers/HomeMonitorRefreshContext";


const DashboardMonitor = ({ title, currentComponentData }) => { 
    
    // Use function to load data:
    //const [isRefreshing, setIsRefreshing] = useState(false);
    const [refreshContext, setRefreshContext] = useContext(HomeMonitorRefreshContext);
    /*
    const update_refresh_status = (is_refresh_triggered, is_refreshing) => {
        setRefreshContext((count) => count % 10 + 1);
    }
    */
    



    return (
        <div className="container full-height">
            <div className="dashboard monitor">
                <DashboardItemMenuHeader title={title}>
                    {/*currentComponentData && // Shu-Yuan Yang 20240828 commented for parent level refresh control
                        <div className="card-editor">
                            <div className="card-button">
                                <button className="button-small" onClick={() => { update_refresh_status(true, true); }} disabled={isRefreshing}>Refresh</button>
                            </div>
                        </div>
                    */}
                </DashboardItemMenuHeader>
                {currentComponentData && (
                    <div className="container content-height">
                        <ErrorBoundary fallback={<h4>Unable to display chart. An error occurred!</h4>}>
                            <DynamicComponentWrapper directory={"charts"} component_name={currentComponentData.ROUTE} title={currentComponentData.DISPLAY_NAME} refreshTriggered={refreshContext} />
                        </ErrorBoundary>
                    </div>
                )}
                
            </div>
        </div>
    );
};


export default DashboardMonitor;