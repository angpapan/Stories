import React, { useState, useEffect } from 'react';
import { api } from '../api';
import './AdminGuard.css';

interface AdminGuardProps {
  children: React.ReactNode;
}

const AdminGuard: React.FC<AdminGuardProps> = ({ children }) => {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    const isAuth = sessionStorage.getItem('admin_authenticated');
    if (isAuth === 'true') {
      setIsAuthenticated(true);
    }
  }, []);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const success = await api.login(password);
      if (success) {
        sessionStorage.setItem('admin_authenticated', 'true');
        setIsAuthenticated(true);
        setError('');
      } else {
        setError('Invalid password');
      }
    } catch (err) {
      setError('An error occurred. Please try again.');
    }
  };

  if (isAuthenticated) {
    return <>{children}</>;
  }

  return (
    <div className="admin-guard-container">
      <div className="admin-guard-card">
        <h2>Admin Access Required</h2>
        <form onSubmit={handleLogin}>
          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input
              type="password"
              id="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Enter admin password"
            />
          </div>
          {error && <div className="error-message">{error}</div>}
          <button type="submit" className="login-button">Login</button>
        </form>
      </div>
    </div>
  );
};

export default AdminGuard;
