import React from 'react';
import { Card as MuiCard, CardContent, Typography, IconButton, Box } from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';

/**
 * Props for the Card component.
 */
interface WorkSessionCardProps {
  /** The title to display at the top of the card. */
  title: string;
  /** The description text to display below the title. */
  description: string;
  /** Optional callback function when the delete button is clicked. */
  onDelete?: () => void;
  /** Optional additional content to render inside the card. */
  children?: React.ReactNode;
}

/**
 * A customizable Card component with a title, description, and an optional delete button.
 * Features a black background and white text.
 */
const WorkSessionCard: React.FC<WorkSessionCardProps> = ({ title, description, onDelete, children }) => {
  return (
    <MuiCard sx={{ backgroundColor: 'black', color: 'white', flexGrow: 1}}>
      <CardContent>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
          <Typography variant="h6" component="div" sx={{ color: 'white' }}>
            {title}
          </Typography>
          {onDelete && (
            <IconButton size="small" onClick={onDelete} sx={{ color: 'white' }} aria-label="delete">
              <DeleteIcon />
            </IconButton>
          )}
        </Box>
        <Typography variant="body2" sx={{ color: 'white' }}>
          {description}
        </Typography>
        {children}
      </CardContent>
    </MuiCard>
  );
};

export default WorkSessionCard;
