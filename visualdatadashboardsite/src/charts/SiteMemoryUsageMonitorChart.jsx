import { PerformanceHelper } from "../auxiliary/objects/performance_helper";
import { useState, useEffect, useMemo } from "react";
import { time_to_string_HHmmss } from "../auxiliary/time/timehelper";
import LineChartStandard from "./LineChartStandard";



const BYTES_PER_MBYTES = 1000000;

const SiteMemoryUsageMonitorChart = ({ title, refreshTriggered }) => {

    const [performanceArr, setPerformanceArr] = useState([]);

    useEffect(() => {
        /*
        if (isRefreshTriggered) {
            setPerformanceArr([]);
            updateRefreshStatus(false, false);
        }
        else {
            updateRefreshStatus(false, false);
        }
        */

        setPerformanceArr([]);
        console.log("reset interval!");
        const interval = setInterval(() => {
            const latestUsage = PerformanceHelper.get_memory_usage();
            setPerformanceArr(oldPerformanceArr => {
                oldPerformanceArr = [...oldPerformanceArr, { timestamp: new Date(), used_memory: latestUsage.usedJSHeapSize, total_memory: latestUsage.totalJSHeapSize }];
                if (oldPerformanceArr.length > 180) oldPerformanceArr = oldPerformanceArr.slice(oldPerformanceArr.length - 180);
                //console.log(oldPerformanceArr);
                return oldPerformanceArr;
            });
        }, 1000);

        return () => clearInterval(interval); // This represents the unmount function, in which you need to clear your interval to prevent memory leaks.
    }, [refreshTriggered, updateRefreshStatus]);


    

    const data_config = useMemo(() => {

        let sampleData = [];
        if (performanceArr.length > 0) {
            for (let i = 0; i < performanceArr.length;) {
                let chunk_size = 0;
                let chunk_used_sum = 0;
                let chunk_total_sum = 0;
                let t = performanceArr[i].timestamp;
                let chunk_upper_bound_time = new Date(t.getTime());
                chunk_upper_bound_time.setSeconds(chunk_upper_bound_time.getSeconds() + 5);
                while (i < performanceArr.length && performanceArr[i].timestamp < chunk_upper_bound_time) {
                    chunk_used_sum += performanceArr[i].used_memory;
                    chunk_total_sum += performanceArr[i].total_memory;
                    ++chunk_size;
                    ++i;
                }

                sampleData.push({ end_time: chunk_upper_bound_time, avg_used_memory: chunk_used_sum / chunk_size, avg_total_memory: chunk_total_sum / chunk_size });
            }
        }

        return {
            labels: sampleData.map((point) => time_to_string_HHmmss(point.end_time)),
            datasets: [{
                label: "Average Used Memory",
                data: sampleData.map((point) => point.avg_used_memory / BYTES_PER_MBYTES),
                backgroundColor: ["rgb(0,128,0)"],
                borderColor: "rgb(0,128,0)",
                borderWidth: 1
            }, {
                label: "Average Total Memory",
                data: sampleData.map((point) => point.avg_total_memory / BYTES_PER_MBYTES),
                backgroundColor: ["rgb(220,20,60)"],
                borderColor: "rgb(220,20,60)",
                borderWidth: 1
            }]
        }

    }, [performanceArr]);


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
                    text: 'time (HH:mm:ss)'
                }
            },
            y: {
                display: true,
                //type: 'logarithmic',
                title: {
                    display: true,
                    text: 'memory used (MB)'
                }
            }
        }
    };


    return (
        <LineChartStandard dataConfig={data_config} dataOptions={data_options} />
    );
};

export default SiteMemoryUsageMonitorChart;
