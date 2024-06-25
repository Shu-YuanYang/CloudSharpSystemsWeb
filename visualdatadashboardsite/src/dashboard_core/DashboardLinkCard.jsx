import { Link } from "react-router-dom";
import DashboardItemCard from "./DashboardItemCard";


const DashboardLinkCard = ({ title, link_path, directory, img_path, size_type }) => (
    <>
        <Link target="_blank" to={`${link_path}?title=${title}`}>
            <DashboardItemCard title={title} directory={directory} img_path={img_path} size_type={size_type} />
        </Link>
    </>
);

export default DashboardLinkCard;