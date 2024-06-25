// Code copied and modified from: https://www.digitalocean.com/community/conceptual-articles/react-loading-components-dynamically-hooks

import { lazy, Suspense, useEffect, useState } from 'react';

const importView = (directory, component_name) =>
    lazy(() =>
        import(`../../${directory}/${(component_name[0] === "/") ? component_name.substr(1) : component_name}.jsx`)
            //.catch(() => import(`./views/NullView`))
    );


const DynamicComponentWrapper = (props) => {
    const [view, setView] = useState(null);
    
    useEffect(() => {
        async function loadViews() {
            const View = await importView(props.directory, props.component_name);
            const viewComp = <View {...props}></View>;
            setView(viewComp);
        }

        loadViews();

    }, [props]);
    
    return (
        <Suspense fallback='Loading...'>
            {view}
        </Suspense>
    );
    
};

export default DynamicComponentWrapper;