import TableStandard from "./TableStandard";
import { export_to_CSV } from "../auxiliary/files/csv_helper";

const TableWithSummary = ({ title, rawData, columnConfig, refreshData }) => {


    const data_donwload = () => {
        export_to_CSV(rawData, title);
    };

    return (
        <div className="r85">
            <div className="summary">
                <span className="summary-item">Record Count: {rawData.length}</span>
                <span className="summary-item">Column Count: {(rawData && rawData.length > 0) ? Object.keys(rawData[0]).length : 0}</span>
                <div className="summary-item right">
                    <button className="summary-button" onClick={data_donwload}>Download CSV</button>
                    <button className="summary-button" onClick={refreshData}>Refresh</button>
                </div>
            </div>
            <div className="container horizontal-centering r90">
                <TableStandard title={title} rawData={rawData} columnConfig={columnConfig} />
            </div>
        </div>
    );
};

export default TableWithSummary;