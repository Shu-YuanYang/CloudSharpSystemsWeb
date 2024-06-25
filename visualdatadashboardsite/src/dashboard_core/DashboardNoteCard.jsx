//import { useMemo } from "react";
//import { api_full_path } from "../endpoints/api/api_helper";


const DashboardNoteCard = ({ title, team_name, sender, message, priority_number, status_code, is_highlighted, size_type }) => {
    /*
    const imgURL = useMemo(() => {
        let imghref = "";
        if (directory === null || directory === undefined || img_path === null || img_path === undefined) return imghref;

        if (directory.startsWith("http")) imghref = api_full_path(directory, img_path);
        else if (img_path.startsWith("http")) imghref = api_full_path(img_path, "");
        else imghref = new URL(`../${directory}/${img_path}`, import.meta.url).href;
        //console.log(imghref);
        return imghref;
    }, [directory, img_path]);
    

    const iconSize = useMemo(() => size_type ? size_type : "icon-img", [size_type]);
    */

    return (
        <div className={`dashboard card ${is_highlighted ? "highlighted" : ""}`}>
            <div className="card-header">
                {/*(size_type === "icon-img-extra-small") && <img src={imgURL} alt="Avatar" className={iconSize}></img>*/}
                <span>{team_name} : {title}</span>
            </div>
            <div>
                <span>{sender}:</span>
                <br />
                <span>{message}</span>
                <br />
            </div>
            <div className="card-footer">
                {status_code !== "TODO" ?
                    <span>{status_code}</span>
                    :
                    <span style={{ color: "blue" }}>{Array.from({ length: priority_number + 1 }, (_, i) => '*').join('')}</span>
                }
                
            </div>
        </div>
    );
};

export default DashboardNoteCard;