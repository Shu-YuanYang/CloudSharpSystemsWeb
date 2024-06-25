// Code copied and modified from: https://stackoverflow.com/questions/45611674/export-2d-javascript-array-to-excel-sheet

export const export_to_CSV = (data_arr, file_name) => {
    var CsvString = "";

    if (data_arr && data_arr.length > 0) {
        // Add column header row:
        for (const key in data_arr[0]) CsvString += key + ',';
        CsvString += "\r\n";

        // Fill data rows:
        data_arr.forEach(function (RowItem, RowIndex) {
            for (const key in data_arr[0]) CsvString += ('"' + String(RowItem[key]).replace(/"/g, "'") + '",');
            CsvString += "\r\n";
        });
    }
    
    CsvString = "data:application/csv," + encodeURIComponent(CsvString);
    var x = document.createElement("A");
    x.setAttribute("href", CsvString);
    x.setAttribute("download", `${file_name}.csv`);
    document.body.appendChild(x);
    x.click();
}