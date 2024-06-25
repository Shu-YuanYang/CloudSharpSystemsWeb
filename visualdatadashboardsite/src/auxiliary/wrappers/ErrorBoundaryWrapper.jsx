// Code copied from: https://www.developerway.com/posts/how-to-handle-errors-in-react
import React from "react";

export class ErrorBoundary extends React.Component {
    constructor(props) {
        super(props);
        // initialize the error state
        this.state = { hasError: false };
    }

    // if an error happened, set the state to true
    static getDerivedStateFromError(error) {
        return { hasError: true };
    }

    componentDidCatch(error, errorInfo) {
        // send error to somewhere here
        console.log(error)
        console.log(errorInfo);
    }

    render() {
        // if error happened, return a fallback component
        if (this.state.hasError) {
            return this.props.fallback;
        }

        return this.props.children;
    }
}