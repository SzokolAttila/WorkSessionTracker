import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import CheckIcon from '@mui/icons-material/Check';

const LoginPage = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const [validations, setValidations] = useState({
    email: false,
    password: false,
  });

  useEffect(() => {
    const isEmailValid = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
    const isPasswordValid = password.length > 0;

    setValidations({
      email: isEmailValid,
      password: isPasswordValid,
    });
  }, [email, password]);

  const isFormValid = Object.values(validations).every(Boolean);

  const handleLogin = (e: React.FormEvent) => {
    e.preventDefault();
    if (isFormValid) {
      // In a real application, you would handle authentication logic here
      alert(`Logging in with: ${email}`);
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
    <div className="min-h-screen flex items-center justify-center bg-gray-100">
      <div className="bg-white p-8 rounded-lg shadow-lg w-full max-w-md">
        <h2 className="text-3xl font-bold text-center text-gray-800 mb-8">Log In</h2>
        <form onSubmit={handleLogin}>
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
              <ValidationItem isMet={validations.email} text="Must be a valid email" />
            </div>
          </div>
          <div className="mb-6">
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
            <div className="mt-2 space-y-1">
              <ValidationItem isMet={validations.password} text="Password is required" />
            </div>
          </div>
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
              Log In
            </button>
            <Link to="/register" className="text-center border border-black bg-white hover:bg-gray-200 active:bg-gray-200 text-black font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline transition-colors">
              Don't have an account yet? Register
            </Link>
          </div>
        </form>
      </div>
    </div>
  );
};

export default LoginPage;
