import React, { useState, useEffect } from 'react';
import CloseIcon from '@mui/icons-material/Close';
import CheckIcon from '@mui/icons-material/Check';
import IconButton from '@mui/material/IconButton';

interface CreateWorkSessionModalProps {
  isOpen: boolean;
  onClose: () => void;
}

const CreateWorkSessionModal: React.FC<CreateWorkSessionModalProps> = ({ isOpen, onClose }) => {
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [description, setDescription] = useState('');
  const [validations, setValidations] = useState({
    startDateRequired: false,
    endDateRequired: false,
    endDateIsLater: false,
  });

  const isFormValid = Object.values(validations).every(Boolean);

  useEffect(() => {
    const startDateRequired = startDate !== '';
    const endDateRequired = endDate !== '';
    const endDateIsLater = startDateRequired && endDateRequired && new Date(endDate) > new Date(startDate);

    setValidations({
      startDateRequired,
      endDateRequired,
      endDateIsLater,
    });
  }, [startDate, endDate]);

  const handleCreate = (e: React.FormEvent) => {
    e.preventDefault();
    if (isFormValid) {
      // In a real app, you would handle the creation logic here
      alert(`Creating session:\nStart: ${startDate}\nEnd: ${endDate}\nDescription: ${description}`);
      onClose(); // Close modal on successful creation
    }
  };

  // Reset state when modal closes
  useEffect(() => {
    if (!isOpen) {
      setStartDate('');
      setEndDate('');
      setDescription('');
      setValidations({
        startDateRequired: false,
        endDateRequired: false,
        endDateIsLater: false,
      });
    }
  }, [isOpen]);

  if (!isOpen) return null;

  const ValidationItem = ({ isMet, text }: { isMet: boolean; text: string }) => (
    <div className="flex items-center text-sm">
      {isMet ? (
        <div className="w-3.5 h-3.5 bg-black text-white flex items-center justify-center mr-2">
          <CheckIcon sx={{ fontSize: '12px' }} />
        </div>
      ) : (
        <div className="w-3.5 h-3.5 border border-gray-400 mr-2" />
      )}
      <span className={isMet ? 'text-gray-800' : 'text-gray-500'}>{text}</span>
    </div>
  );

  return (
    <div
      onClick={onClose}
      className="fixed inset-0 bg-black/50 bg-opacity-50 z-50 flex items-center justify-center p-4"
    >
      {/* Modal Panel */}
      <div
        onClick={(e) => e.stopPropagation()}
        className="bg-white rounded-lg shadow-lg w-full max-w-lg p-6 relative animate-fade-in-up"
      >
          <div className="flex justify-between items-center mb-6">
            <h2 className="text-2xl font-bold text-gray-800">New Work Session</h2>
            <IconButton onClick={onClose} aria-label="Close modal" className="absolute top-2 right-2">
              <CloseIcon />
            </IconButton>
          </div>

          <form onSubmit={handleCreate}>
            <div className="space-y-4">
              <div>
                <label htmlFor="startDate" className="block text-gray-700 text-sm font-bold mb-2">Start Date & Time</label>
                <input
                  type="datetime-local"
                  id="startDate"
                  value={startDate}
                  onChange={(e) => setStartDate(e.target.value)}
                  className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
                  placeholder="Select start date"
                  required
                />
                <div className="mt-2 space-y-1">
                  <ValidationItem isMet={validations.startDateRequired} text="Start date is required" />
                </div>
              </div>
              <div>
                <label htmlFor="endDate" className="block text-gray-700 text-sm font-bold mb-2">End Date & Time</label>
                <input
                  type="datetime-local"
                  id="endDate"
                  value={endDate}
                  onChange={(e) => setEndDate(e.target.value)}
                  className={`shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline ${endDate && startDate && !validations.endDateIsLater ? 'border-red-500' : ''}`}
                  placeholder="Select end date"
                  required
                />
                <div className="mt-2 space-y-1">
                  <ValidationItem isMet={validations.endDateRequired} text="End date is required" />
                  <ValidationItem isMet={validations.endDateIsLater} text="End date must be after start date" />
                </div>
              </div>
              <div>
                <label htmlFor="description" className="block text-gray-700 text-sm font-bold mb-2">Description (Optional)</label>
                <textarea id="description" value={description} onChange={(e) => setDescription(e.target.value)} className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline" rows={4} placeholder="What did you work on?" />
              </div>
            </div>
            <div className="mt-4">
              <button type="submit" disabled={!isFormValid} className={`w-full font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline transition-colors ${isFormValid ? 'bg-black hover:bg-black/75 text-white' : 'bg-gray-400 text-gray-200 cursor-not-allowed'}`}>Create</button>
            </div>
          </form>
      </div>
    </div>
  );
};

export default CreateWorkSessionModal;
