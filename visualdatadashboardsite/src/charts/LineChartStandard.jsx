import { Chart as ChartJS } from 'chart.js/auto';
import { Line } from 'react-chartjs-2';




/*
    data format: {
        x: Array[],
        y: Array[],
        sample_semantics: String
    }
*/
const LineChartStandard = ({ dataConfig, dataOptions }) => {


    
    

    return (
        <Line
            data={dataConfig}
            options={dataOptions}
        />
    );
};

export default LineChartStandard;