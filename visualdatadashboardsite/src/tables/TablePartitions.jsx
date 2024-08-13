import TableStandard from "./TableStandard";
import { export_to_CSV } from "../auxiliary/files/csv_helper";

const TablePartitionsWithSummary = ({ title, rawData, columnConfig, refreshData }) => {

    const data_donwload = (dataList, key) => {
        export_to_CSV(dataList, key);
    };

    return (
        <div className="r85">
            {rawData.map(group => <>
                <div className="menu-header large">
                    <span>{group.key}</span>
                    <div className="summary-item right">
                        <button className="summary-button" onClick={() => { data_donwload(group.dataList, group.key); }}>Download CSV</button>
                        <button className="summary-button" onClick={refreshData}>Refresh</button>
                    </div>
                </div>
                <TableStandard key={group.key} title={group.key} rawData={group.dataList} columnConfig={columnConfig} />
            </>)}
            
                
            
        </div>
    );
};

export default TablePartitionsWithSummary;