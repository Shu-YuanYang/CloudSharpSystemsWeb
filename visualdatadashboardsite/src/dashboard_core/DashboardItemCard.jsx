//import reactLogo from '../assets/react.svg';
//import UseIcon from '../endpoints/local_asset_load/UseIcon';
import { useMemo } from "react";
import { api_full_path } from "../endpoints/api/api_helper";


const DashboardItemCard = ({ title, directory, img_path, is_highlighted, size_type }) => {

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

    return (
        <div className={`dashboard card ${is_highlighted ? "highlighted" : ""}`}>
            {!(size_type === "icon-img-extra-small") && 
                (<div className="card-img">
                    {
                        /*
                            <div className="card-img-element icon">
                                <UseIcon className="icon" width="100%" height="70" name={img_path}></UseIcon>
                            </div>
                        */
                    }

                    <div className="icon">
                        <img src={imgURL} alt="Avatar" className={iconSize}></img>
                    </div>

                </div>)
            }
            {title && 
                <div className="card-footer">
                    {(size_type === "icon-img-extra-small") && <img src={imgURL} alt="Avatar" className={iconSize}></img>}
                    <span>{title}</span>
                </div>
            }
        </div>
    );
};

export default DashboardItemCard;