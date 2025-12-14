import Card from '../components/Card';
import FloatingActionButton from '../components/FAB';

const HomePage = () => {
    const handleDelete = (cardTitle: string) => {
        // In a real app, you would handle the delete logic here
        alert(`Delete action triggered for: ${cardTitle}`);
    };

    const handleFabClick = () => {
        // This could open a modal or navigate to a "new item" page
        alert('FAB clicked!');
    };

    return (
        <>
            <div className="space-y-4">
                <Card
                    title="Work Session #1"
                    description="Implemented the main layout and a reusable AppBar component using a mix of Tailwind and Material-UI for icons."
                    onDelete={() => handleDelete('Work Session #1')}
                />
                <Card
                    title="Work Session #2"
                    description="Created a fully responsive and reusable Card component styled exclusively with Tailwind CSS."
                    onDelete={() => handleDelete('Work Session #2')}
                />
                <Card
                    title="Future Task: API Integration"
                    description="Plan and implement the data fetching logic to connect the UI with the backend API."
                    onDelete={() => handleDelete('Future Task: API Integration')}
                />
            </div>
            <FloatingActionButton onClick={handleFabClick} />
        </>
    );
};

export default HomePage;
