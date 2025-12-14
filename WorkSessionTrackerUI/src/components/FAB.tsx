import React from 'react';
import AddIcon from '@mui/icons-material/Add';

interface FABProps {
  onClick: () => void;
}

const FloatingActionButton: React.FC<FABProps> = ({ onClick }) => {
  return (
    <button
      onClick={onClick}
      className="fixed bottom-6 right-6 bg-black text-white w-14 h-14 rounded-full shadow-lg flex items-center justify-center transition-transform duration-300 hover:scale-110 active:outline-none active:ring-2 active:ring-offset-2 active:ring-black"
      aria-label="Add new item"
    >
      <AddIcon fontSize="large" />
    </button>
  );
};

export default FloatingActionButton;
