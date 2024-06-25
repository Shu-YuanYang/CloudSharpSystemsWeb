// Code copied from https://medium.com/@erickhoury/react-dynamically-importing-svgs-and-render-as-react-component-b764b6475896
// SVG React import syntax: https://www.npmjs.com/package/vite-plugin-svgr
import { useState, useEffect, useRef } from 'react';

const UseIcon = ({ className, name, width = 16, height = 16, fill = "#000" }) => {
    const ImportedIconRef = useRef(null);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        setLoading(true);

        const importIcon = async () => {
            try {
                const { default: namedImport } = await import(`../../assets/${name}.svg?react`);
                ImportedIconRef.current = namedImport;
            } finally {
                setLoading(false);
            }
        };

        importIcon();
    }, [name]);

    if (!loading && ImportedIconRef.current) {
        const { current: ImportedIcon } = ImportedIconRef;
        return <ImportedIcon className={className} width={width} height={height} fill={fill} />;
    }

    return null;
};

export default UseIcon;
