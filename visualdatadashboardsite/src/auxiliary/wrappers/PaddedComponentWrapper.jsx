

const PaddedComponentWrapper = (props) => (
    <div className={`itempad ${props.size? props.size : "medium"}`}>
        {props.children}
    </div>
);

export default PaddedComponentWrapper;