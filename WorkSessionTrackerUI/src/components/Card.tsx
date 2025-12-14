import React from 'react';
import DeleteIcon from '@mui/icons-material/Delete';

interface CardProps {
  title: string;
  description: string;
  onDelete: () => void;
}

const Card: React.FC<CardProps> = ({ title, description, onDelete }) => {
  return (
    <div className="bg-black rounded-lg shadow-lg p-4 transition-shadow duration-300 hover:shadow-xl">
      <div className="flex justify-between items-start mb-2">
        <h3 className="text-lg font-bold text-white">{title}</h3>
        <button
          onClick={onDelete}
          className="text-white border border-white rounded-lg hover:border-red-500 active:border-red-500 active:text-red-500 hover:text-red-500 focus:outline-none p-2"
          aria-label={`Delete ${title}`}
        >
          <DeleteIcon fontSize="small" />
        </button>
      </div>
      <p className="text-white">{description}</p>
    </div>
  );
};

export default Card;
