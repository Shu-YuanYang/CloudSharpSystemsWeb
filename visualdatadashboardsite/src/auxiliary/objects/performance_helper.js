export class PerformanceHelper {

    static get_memory_usage() { 
        return window.performance? window.performance.memory : null;
    }


}