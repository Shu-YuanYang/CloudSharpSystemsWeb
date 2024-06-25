import { useEffect } from "react";

export const usePageTitle = (title) => {

    //const [docTitle, setDocTitle] = useState("CloudSharp Dashboard");

    useEffect(() => {
        document.title = `Dashboard - ${title}`;
        //setDocTitle(document.title);
    }, [title]);
}

