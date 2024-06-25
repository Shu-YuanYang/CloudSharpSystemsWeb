import { TextSearchFilter } from "../../tables/TableFilters";


export class TableHelper {

    static map_columns(map_obj) {
        let cols = [];
        for (const key in map_obj) {
            cols.push({ Header: map_obj[key], accessor: key, Filter: TextSearchFilter });
        }
        return cols;
    }

    static format_columns(table_header, map_obj) {
        return [{ Header: table_header, columns: TableHelper.map_columns(map_obj) }];
    }

    static format_key(key) {
        let val = key;
        val = val.replace(/_/g, " ");
        val = val.replace(/-/g, " ");
        return val;
    }

    static format_columns_from_data(data, table_header, map_obj) {
        if (!data || data.length === 0) return TableHelper.format_columns("", map_obj);
        let data_obj = data[0];
        for (const key in data_obj) {
            if (!(key in map_obj)) {
                //let val = key;
                //val = val.replace(/_/g, " ");
                //val = val.replace(/-/g, " ");
                map_obj[key] = TableHelper.format_key(key);
            }
        }
        return TableHelper.format_columns(table_header, map_obj);
    }

    static get_page_size_options_array(rawDataSize) {
        const size_increment = 10;
        let current_size = size_increment;
        let page_size_arr = [current_size];
        while (current_size < rawDataSize && current_size < 50) {
            current_size += size_increment;
            page_size_arr.push(current_size);
        }
        return page_size_arr;
    }
    
}

