import React from 'react';
import AppBar from '../AppBar'; 
import { Box, Toolbar } from '@mui/material';

/**
 * Props for the PageLayout component.
 */
interface PageLayoutProps {
  /** The title to display in the AppBar. */
  title: string;
  /** The content of the page to be rendered below the AppBar. */
  children: React.ReactNode;
  /** Optional callback function when the hamburger menu icon in the AppBar is clicked. */
  onMenuClick?: () => void;
}

/**
 * A general page layout component that includes an AppBar at the top
 * and renders its children as the main page content below it.
 */
const PageLayout: React.FC<PageLayoutProps> = ({ title, children }) => {
  const handleMenuClick = () => {
    // This is where you'd typically open a sidebar/drawer
    console.log('Hamburger menu clicked on Home Page!');
    alert('Menu Clicked!');
  };

  return (
    <Box height={"100vh"} display="flex" flexDirection="column">
      <AppBar title={title} onMenuClick={handleMenuClick} />
      <Toolbar /> {/* Spacer to offset the fixed AppBar */}
      <Box component="main" sx={{ p: 4}}>
        {children}
      </Box>
    </Box>
  );
};

export default PageLayout;
