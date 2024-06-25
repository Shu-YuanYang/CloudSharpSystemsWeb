// Pagination code copied from https://codesandbox.io/p/sandbox/react-table-usepagination-blp9xv?file=%2Fsrc%2Fcomponents%2FDataTable.js%3A17%2C5-17%2C20
import { useState, useEffect, useMemo } from "react";
import { TableHelper } from "../auxiliary/objects/table_helper";
import { useTable, usePagination, useFilters } from "react-table";
import TooltipWrapper from "../auxiliary/wrappers/TooltipWrapper";
import Tooltip from "../auxiliary/objects/Tooltip";


const TableStandard = ({ title, rawData, columnConfig }) => {

    const columns_config = useMemo(() => {
        const config_arr = TableHelper.format_columns_from_data(rawData, title, columnConfig);
        if (config_arr && config_arr.length > 0) return config_arr[0].columns;
        return config_arr;
    }, [title, rawData, columnConfig]);

    const page_size_options = useMemo(() => TableHelper.get_page_size_options_array(rawData.length), [rawData]);



    const [columnExpandedList, setColumnExpandedList] = useState([]);

    const defaultColumn = useMemo(
        () => ({
            // Let's set up our default Filter UI
            Filter: ""
        }),
        []
    );
    const filterTypes = useMemo(
        () => ({
            /*rankedMatchSorter: matchSorterFn*/
        }),
        []
    );

    
    const {
        getTableProps,
        getTableBodyProps,
        headerGroups,
        prepareRow,
        rows,

        // add pagination support
        page,
        canPreviousPage,
        canNextPage,
        pageOptions,
        pageCount,
        gotoPage,
        nextPage,
        previousPage,
        setPageSize,
        state: { pageIndex, pageSize }
    } = useTable(
        {
            columns: columns_config,
            data: rawData,
            initialState: { pageSize: 10, pageIndex: 0 },

            defaultColumn,
            filterTypes
        },
        useFilters,
        usePagination
    );



    useEffect(() => {
        setColumnExpandedList(columns_config.map((config, index) => { return index < 10; }));
    }, [columns_config]);


    const change_column_expanded_status = (index) => {
        setColumnExpandedList(prevArr => {
            let newArr = [...prevArr];
            newArr[index] = !newArr[index];
            return newArr;
        });
    }




    const columnHeader = ({ column, columnIndex }) => (
        <div className="card-editor">
            <div className="container horizontal-centering">
                {columnExpandedList[columnIndex] && column.render("Header")}
                {/* button below is only used to reserve space and does not provide any functionality */}
                <button className="summary-button dummy-invisible" disabled={true}>{columnExpandedList[columnIndex] ? "X" : "..."}</button>
            </div>
            <div className="card-button-top">
                <TooltipWrapper>
                    <button className="summary-button" onClick={() => { change_column_expanded_status(columnIndex); }}>
                        {columnExpandedList[columnIndex] ? "X" : "..."}
                        {!columnExpandedList[columnIndex] && <Tooltip text={column.render("Header")} />}
                    </button>
                </TooltipWrapper>
            </div>
            {columnExpandedList[columnIndex] && <div className="container horizontal-centering column-header-filter">{column.canFilter ? column.render("Filter") : null}</div>}
        </div>
    );


    const tableRow = ({ row }) => (
        <tr key={row.original.id} {...row.getRowProps()}>
            {
                row.cells.map((cell, i) => {
                    return (
                        <td key={i} {...cell.getCellProps()}>
                            {columnExpandedList[i] && <div className="table-cell">{cell.render("Cell")}</div>}
                        </td>
                    );
                })
            }
        </tr>
    );

    return (
        <div className="container full-width full-height">
            <div className="container table">
                <table {...getTableProps()}>
                    <thead className="sticky">
                        {headerGroups.map((headerGroup) => (
                            <tr key={headerGroup.id} {...headerGroup.getHeaderGroupProps()}>
                                {headerGroup.headers.map((column, i) => (
                                    <th key={column.id} {...column.getHeaderProps()}>
                                        {columnHeader({ column: column, columnIndex: i })}
                                    </th>
                                ))}
                            </tr>
                        ))}
                    </thead>
                    <tbody {...getTableBodyProps()}>
                        {
                            page.map((row) => {
                                prepareRow(row);
                                return tableRow({ row });
                            })
                        }
                    </tbody>
                </table>
            </div>
            <br />

            <div className="pagination">
                <button onClick={() => gotoPage(0)} disabled={!canPreviousPage}>{"<<"}</button>{" "}
                <button onClick={() => previousPage()} disabled={!canPreviousPage}>{"<"}</button>{" "}
                <button onClick={() => nextPage()} disabled={!canNextPage}>{">"}</button>{" "}
                <button onClick={() => gotoPage(pageCount - 1)} disabled={!canNextPage}>{">>"}</button>{" "}
                <span>Page{" "}<strong>{pageIndex + 1} of {pageCount/*pageOptions.length*/}</strong>{" "}</span>
                <span>| Go to page:{" "}
                    <input
                        type="number"
                        defaultValue={pageIndex + 1}
                        onChange={(e) => {
                            let page = e.target.value ? Number(e.target.value) - 1 : 0;
                            if (page < 0) page = 0;
                            if (page >= pageCount) page = pageCount - 1;
                            e.target.value = page + 1;
                            gotoPage(page);
                        }}
                        style={{ width: "100px" }}
                    />
                </span>{" "}
                <select
                    value={pageSize}
                    onChange={(e) => { setPageSize(Number(e.target.value)); }}
                >
                    {page_size_options.map((pageSize) => (
                        <option key={pageSize} value={pageSize}>Show {pageSize}</option>
                    ))}
                </select>
            </div>
        </div>
        
    );
};

export default TableStandard;
