import LineChartStandard from "./LineChartStandard";
import { useMemo, useEffect } from "react";
import useFetch from "../endpoints/api/useFetch";
import { api_full_path, get_api } from "../endpoints/api/api_helper";
import { APIEndpoints } from "../site_config.json";
import { time_to_string_HHmm, string_expression_to_time_obj } from "../auxiliary/time/timehelper";


const DBLatencyMonitorChart = ({ title, refreshTriggered, updateRefreshStatus }) => {

    const { data: sampleData, refreshData: refreshSampleData, isPending, error } = useFetch(api_full_path(APIEndpoints.CloudSharpMicroService.url, get_api(APIEndpoints.CloudSharpMicroService, "get_recent_db_status_logs").path));
    
    useEffect(() => {
        refreshSampleData();
    }, [refreshTriggered, refreshSampleData]);


    const data_config = useMemo(() => {
        if (isPending || error || sampleData.length === 0) return {};
        return {
            labels: sampleData[0].latency_statistics.map((point) => time_to_string_HHmm(string_expression_to_time_obj(point.END_TIME))),
            datasets: sampleData.map((host_latency, index) => {
                let increment = 30 * index;
                let avg_color = [75 + increment, 192 + increment, 192 + increment];
                let max_color = [220 + increment, 100 + increment, 140 + increment];

                let latency_lst = [{
                    label: `${host_latency.host} AVG`,
                    data: host_latency.latency_statistics.map((point) => point.AVG_LATENCY),
                    backgroundColor: [`rgb(${avg_color[0]},${avg_color[1]},${avg_color[2]})`],
                    borderColor: `rgb(${avg_color[0]},${avg_color[1]},${avg_color[2]})`,
                    borderWidth: 1
                }, {
                    label: `${host_latency.host} MAX`,
                    data: host_latency.latency_statistics.map((point) => point.MAX_LATENCY),
                    backgroundColor: [`rgb(${max_color[0]},${max_color[1]},${max_color[2]})`],
                    borderColor: `rgb(${max_color[0]},${max_color[1]},${max_color[2]})`,
                    borderWidth: 1,
                    hidden: true
                }];

                return latency_lst;
            }).reduce((acc, lst) => {
                acc.push(...lst);
                return acc;
            }, [])
        }
    }, [sampleData, isPending, error]);

    /*
    const data_config = useMemo(() => {
        if (isPending || error) return {};
        return {
            labels: sampleData.map((point) => time_to_string_HHmm(string_expression_to_time_obj(point.END_TIME))),
            datasets: [{
                label: "Minimum Query Latency",
                data: sampleData.map((point) => point.MIN_LATENCY),
                backgroundColor: ["rgb(75,192,192)"],
                borderColor: "rgb(75,192,192)",
                borderWidth: 1
            }, {
                label: "Averrage Query Latency",
                data: sampleData.map((point) => point.AVG_LATENCY),
                backgroundColor: ["rgb(0,191,255)"],
                borderColor: "rgb(0,191,255)",
                borderWidth: 1
            }, {
                label: "Maximum Query Latency",
                data: sampleData.map((point) => point.MAX_LATENCY),
                backgroundColor: ["rgb(220,20,60)"],
                borderColor: "rgb(220,20,60)",
                borderWidth: 1
            }]
        }
    }, [sampleData, isPending, error]);
    */


    const data_options = {
        plugins: {
            title: {
                display: true,
                text: title,
                position: "bottom",
                font: {
                    weight: "bold",
                    size: 18
                }
            },
            legend: {
                display: true
            }
        },
        scales: {
            x: {
                display: true,
                title: {
                    display: true,
                    text: 'time (HH:mm)'
                }
            },
            y: {
                display: true,
                type: 'logarithmic',
                title: {
                    display: true,
                    text: 'latency (ms)'
                }
            }
        }
    };


    const ChartElem = (isPending) ? <div><span>Loading...</span></div> : <LineChartStandard dataConfig={data_config} dataOptions={data_options} />;


    return ChartElem;

}

export default DBLatencyMonitorChart;