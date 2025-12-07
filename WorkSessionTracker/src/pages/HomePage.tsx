import React from 'react';
import PageLayout from '../components/layout/PageLayout';
import { Box } from '@mui/material';
import WorkSessionCard from '../components/WorkSessionCard';

/**
 * The main home page component for the application.
 * It uses the PageLayout for consistent top-level structure.
 */
const HomePage: React.FC = () => {

  const deleteCard = () => {
    alert('Delete clicked!');
  }

  return (
    <PageLayout title="Work Sessions">
      <Box sx={{ width: '100%', gap: 2, display: 'flex', flexDirection: 'column' }}>
        <WorkSessionCard title='12:00-18:00' onDelete={deleteCard} description='Was working on this and that' />
      </Box>
    </PageLayout>
  );
};

export default HomePage;
