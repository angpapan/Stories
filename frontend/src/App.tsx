import { BrowserRouter, Routes, Route } from 'react-router-dom';
import StoriesList from './pages/StoriesList';
import StoryEditor from './pages/StoryEditor';
import StoryPlaythroughs from './pages/StoryPlaythroughs';
import Playthrough from './pages/Playthrough';
import NotFound from './pages/NotFound';
import AdminGuard from './components/AdminGuard';

function App() {
  return (
    <BrowserRouter>
      <div className="app-container">
        <header className="app-header">
          <h1>Pap Stories</h1>
        </header>
        <main className="app-main">
          <Routes>
            <Route path="/" element={<NotFound />} />
            <Route path="*" element={<NotFound />} />
            <Route path="/admin" element={<AdminGuard><StoriesList /></AdminGuard>} />
            <Route path="/admin/story/:id" element={<AdminGuard><StoryEditor /></AdminGuard>} />
            <Route path="/admin/story/:id/playthroughs" element={<AdminGuard><StoryPlaythroughs /></AdminGuard>} />
            <Route path="/play/:id" element={<Playthrough />} />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  );
}

export default App;
