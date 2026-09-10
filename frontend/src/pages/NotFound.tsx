import React, { useEffect } from 'react';

const NotFound: React.FC = () => {
  useEffect(() => {
    document.title = 'Page Not Found';
  }, []);

  return (
    <div className="not-found-container" style={{ textAlign: 'center', padding: '4rem 2rem' }}>
      <h2>404 - Page Not Found</h2>
      <p>The page you are looking for does not exist.</p>
    </div>
  );
};

export default NotFound;
