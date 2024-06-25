export function time_to_string_HHmm(DateObj) {
    var hours = DateObj.getHours();

    // Minutes part from the timestamp
    var minutes = "0" + DateObj.getMinutes();

    var formattedTime = hours + ':' + minutes.slice(-2);
    return formattedTime;
}

export function time_to_string_HHmmss(DateObj) {
    

    // Seconds part from the timestamp
    var seconds = "0" + DateObj.getSeconds();

    // Will display time in 10:30:23 format
    var formattedTime = time_to_string_HHmm(DateObj) + ':' + seconds.slice(-2);
    return formattedTime;

}

export function string_expression_to_time_obj(str) {
    return new Date(Date.parse(str));
}