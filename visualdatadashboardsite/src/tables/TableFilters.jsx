// Code copied from https://codesandbox.io/p/sandbox/github/ncoughlin/react-table_useFilter/tree/main/?file=%2Fsrc%2Futilities%2Ffilters.js%3A1%2C1-49%2C1
import React from "react";

// text search input
export const TextSearchFilter = ({ column: { filterValue, preFilteredRows, setFilter } }) => {
    return (
        <input
            value={filterValue || ""}
            onChange={(e) => {
                setFilter(e.target.value || undefined); // Set undefined to remove the filter entirely
            }}
            placeholder={`Search...`}
        />
    );
}

// a dropdown list filter
export const DropdownFilter = ({ column: { filterValue, setFilter, preFilteredRows, id } }) => {
    // Calculate the options for filtering
    // using the preFilteredRows
    const options = React.useMemo(() => {
        const options = new Set();
        preFilteredRows.forEach((row) => {
            options.add(row.values[id]);
        });
        return [...options.values()];
    }, [id, preFilteredRows]);

    // Render a multi-select box
    return (
        <select
            value={filterValue}
            onChange={(e) => {
                setFilter(e.target.value || undefined);
            }}
        >
            <option value="">All</option>
            {options.map((option, i) => (
                <option key={i} value={option}>
                    {option}
                </option>
            ))}
        </select>
    );
}
