import React from 'react';
import ReactDom from 'react-dom/client';

const App = () => {
    const [data, setData] = React.useState([]);
    React.useEffect(() => {
        const eventSource = new EventSource(
            "http://localhost:7225/api/SSE"
        );

        eventSource.onmessage = function (event) {
            try {

                const response = event.data.replace(/\\n/g, '\n').replace(/\\"/g, '"');
                const dd = JSON.parse(response);
                setData(prevData => [...prevData, dd]);
            } catch (e) { console.log("Something went wrong:", e) }
        };

        eventSource.onerror = function (error) {
            console.error("SSE error:", error);
        };

        return () => {
            eventSource.close(); // Clean up the event source on unmount
        };
    }, []);

    return <>
        <h1>Total market data: {data.length}</h1>
        {data.map((d, i) => {
            console.log(d)
            return <div key={i}>

                <pre>
                    {JSON.stringify(d["Meta Data"])}
                </pre>
            </div>
        })}
    </>
}


const el = document.getElementById('root');
const root = ReactDom.createRoot(el);

root.render(<App />);