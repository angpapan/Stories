import { BrowserRouter, Routes, Route } from 'react-router-dom';
import StoriesList from './pages/StoriesList';
import StoryEditor from './pages/StoryEditor';

function App() {
  return (
    <BrowserRouter>
      <div className="app-container">
        <header className="app-header">
          <h1>Interactive Story Admin</h1>
        </header>
        <main className="app-main">
          <Routes>
            <Route path="/" element={<StoriesList />} />
            <Route path="/story/:id" element={<StoryEditor />} />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  );
}

export default App;
