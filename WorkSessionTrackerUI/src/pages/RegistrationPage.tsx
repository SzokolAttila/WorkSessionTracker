import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import CheckIcon from '@mui/icons-material/Check';

const RegistrationPage = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [role, setRole] = useState('Student');

  const [validations, setValidations] = useState({
    email: false,
    length: false,
    lowercase: false,
    uppercase: false,
    digit: false,
    special: false,
    match: false,
  });

  const navigate = useNavigate();

  useEffect(() => {
    const isEmailValid = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
    const isLengthValid = password.length >= 8;
    const hasLowercase = /[a-z]/.test(password);
    const hasUppercase = /[A-Z]/.test(password);
    const hasDigit = /\d/.test(password);
    const hasSpecial = /[!@#$%^&*(),.?":{}|<>]/.test(password);
    const doPasswordsMatch = password !== '' && password === confirmPassword;

    setValidations({
      email: isEmailValid,
      length: isLengthValid,
      lowercase: hasLowercase,
      uppercase: hasUppercase,
      digit: hasDigit,
      special: hasSpecial,
      match: doPasswordsMatch,
    });
  }, [email, password, confirmPassword]);

  const isFormValid = Object.values(validations).every(Boolean);

  const handleRegister = (e: React.FormEvent) => {
    e.preventDefault();
    if (isFormValid) {
      // In a real app, you would call your registration API here
      alert(`Registration successful for ${role}: ${email}`);
      navigate('/login');
    } else {
      alert('Please ensure all requirements are met.');
    }
  };

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
    <div className="min-h-screen flex items-center justify-center bg-gray-100 py-8">
      <div className="bg-white p-8 rounded-lg shadow-lg w-full max-w-md">
        <h2 className="text-3xl font-bold text-center text-gray-800 mb-8">Register</h2>
        <form onSubmit={handleRegister}>
          {/* Email */}
          <div className="mb-4">
            <label htmlFor="email" className="block text-gray-700 text-sm font-bold mb-2">
              Email Address
            </label>
            <input
              type="email"
              id="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
              placeholder="your.email@example.com"
              required
            />
            <div className="mt-2 space-y-1">
              <ValidationItem isMet={validations.email} text="Must be a valid email address" />
            </div>
          </div>

          {/* Password */}
          <div className="mb-4">
            <label htmlFor="password" className="block text-gray-700 text-sm font-bold mb-2">
              Password
            </label>
            <input
              type="password"
              id="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
              placeholder="******************"
              required
            />
            <div className="mt-2 grid grid-cols-1 sm:grid-cols-2 gap-x-4 gap-y-1">
              <ValidationItem isMet={validations.length} text="At least 8 characters" />
              <ValidationItem isMet={validations.lowercase} text="Contains a lowercase letter" />
              <ValidationItem isMet={validations.uppercase} text="Contains an uppercase letter" />
              <ValidationItem isMet={validations.digit} text="Contains a digit" />
              <ValidationItem isMet={validations.special} text="Contains a special character" />
            </div>
          </div>

          {/* Confirm Password */}
          <div className="mb-6">
            <label htmlFor="confirmPassword" className="block text-gray-700 text-sm font-bold mb-2">
              Password Again
            </label>
            <input
              type="password"
              id="confirmPassword"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
              placeholder="******************"
              required
            />
            <div className="mt-2 space-y-1">
              <ValidationItem isMet={validations.match} text="Passwords must match" />
            </div>
          </div>

          {/* Role Selection */}
          <div className="mb-6">
            <span className="block text-gray-700 text-sm font-bold mb-2">I am a...</span>
            <div className="flex items-center space-x-6">
              <label className="flex items-center cursor-pointer">
                <input type="radio" name="role" value="Student" checked={role === 'Student'} onChange={(e) => setRole(e.target.value)} className="h-4 w-4 text-black focus:ring-black border-gray-300" />
                <span className="ml-2 text-gray-700">Student</span>
              </label>
              <label className="flex items-center cursor-pointer">
                <input type="radio" name="role" value="Company" checked={role === 'Company'} onChange={(e) => setRole(e.target.value)} className="h-4 w-4 text-black focus:ring-black border-gray-300" />
                <span className="ml-2 text-gray-700">Company</span>
              </label>
            </div>
          </div>

          {/* Buttons */}
          <div className="flex flex-col space-y-4">
            <button
              type="submit"
              disabled={!isFormValid}
              className={`font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline transition-colors ${
                isFormValid
                  ? 'bg-black hover:bg-black/75 text-white'
                  : 'bg-gray-400 text-gray-200 cursor-not-allowed'
              }`}
            >
              Register
            </button>
            <Link to="/login" className="text-center border border-black bg-white hover:bg-gray-200 text-black font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline transition-colors">
              Already have an account? Log In
            </Link>
          </div>
        </form>
      </div>
    </div>
  );
};

export default RegistrationPage;
