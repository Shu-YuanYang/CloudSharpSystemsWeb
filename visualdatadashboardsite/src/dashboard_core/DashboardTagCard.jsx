
import DashboardItemCard from "./DashboardItemCard";


const DashboardTagCard = ({ title, callback, directory, img_path, is_highlighted }) => (
    <>
        <a className="clickable" onClick={callback}>
            <DashboardItemCard title={title} directory={directory} img_path={img_path} is_highlighted={is_highlighted} />
        </a>
    </>
);

export default DashboardTagCard;