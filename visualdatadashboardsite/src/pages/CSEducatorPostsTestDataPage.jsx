// Data source: cseducators.stackexchange.com.7z at https://archive.org/download/stackexchange/
//import { posts } from "../sample_sortable/cseducators_stackexchange_posts.json";
import { useMemo } from "react";

const posts = {
    row: null
}

const transform_json_to_sql = (json_data_array) => {

    console.log(json_data_array.length);

    let sql_str_arr = [];
    let header_str = "";
    if (json_data_array && json_data_array.length > 0) {
        for (const key in json_data_array[0]) header_str += key.slice(1) + ", ";
        header_str = header_str.slice(0, -1); // Remove the last character
        header_str = header_str.slice(0, -1); // Remove the last character

        // Fill data rows:
        sql_str_arr = json_data_array.map((RowItem, RowIndex) => {
            let sql_str = "INSERT INTO EXTERNAL_STACKEXCHANGE.TB_CS_EDUCATOR_RAW SELECT ";
            for (const key in json_data_array[0]) {
                // process data:
                let substr = RowItem[key] ? RowItem[key] : "";
                substr = substr.replace(/'/g, "''"); // escape '
                substr = substr.replace(/\n/g, "\\n"); // escape \n
                sql_str += "'" + substr + "', ";
            }
            sql_str = sql_str.slice(0, -1); // Remove the last character
            sql_str = sql_str.slice(0, -1); // Remove the last character
            sql_str += ";";
            //if ((RowIndex + 1) % 50 === 0) sql_str += "\r\nCOMMIT;";
            return sql_str;
        });
    }

    console.log(sql_str_arr.slice(0, 50));

    return {
        columns: header_str,
        sql: sql_str_arr
    };
}

const download_sql_file = (data_dump) => {
    let textStr = "--" + data_dump.columns + "\r\n";
    data_dump.sql.forEach((row, index) => {
        textStr += row;
        textStr += "\r\n";
    })
    textStr = "data:text/plain;charset=utf-8," + encodeURIComponent(textStr);
    var x = document.createElement("A");
    x.setAttribute("href", textStr);
    x.setAttribute("download", `temp_sql.sql`);
    document.body.appendChild(x);
    x.click();
}


const CSEducatorPostsTestDataPage = () => {


    const data_dump = useMemo(() => transform_json_to_sql(posts.row), []);
    const displayed_data = useMemo(() => data_dump.sql.slice(0, 50), [data_dump]);
    

    return (
        <div style={{ height: "100vh", overflowY: "scroll" }}>
            <h1>Hello Test</h1>
            <button onClick={() => { download_sql_file(data_dump); } }>Download SQL Insert</button>
            <h4>{data_dump.columns}</h4>
            {displayed_data.map((row, index) =>
                <div key={index}>
                    <span>{row}</span>
                    <br />
                    <br />
                </div>)
            }
        </div>
        
    );
};

export default CSEducatorPostsTestDataPage;
