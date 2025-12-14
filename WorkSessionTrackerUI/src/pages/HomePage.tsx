import React, { useState } from 'react';
import Card from '../components/Card';
import FloatingActionButton from '../components/FAB';
import CreateWorkSessionModal from '../components/CreateWorkSessionModal';

const HomePage = () => {
    const [isModalOpen, setModalOpen] = useState(false);

    const handleDelete = (cardTitle: string) => {
        // In a real app, you would handle the delete logic here
        alert(`Delete action triggered for: ${cardTitle}`);
    };

    const handleFabClick = () => {
        setModalOpen(true);
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
            <CreateWorkSessionModal isOpen={isModalOpen} onClose={() => setModalOpen(false)} />
        </>
    );
};

export default HomePage;
