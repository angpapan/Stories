import React, { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { api, Story } from '../api';
import { PlusCircle, Book, Copy } from 'lucide-react';
import '../index.css';

const StoriesList: React.FC = () => {
  const [stories, setStories] = useState<Story[]>([]);
  const [isCreating, setIsCreating] = useState(false);
  const [newTitle, setNewTitle] = useState('');
  const [newDescription, setNewDescription] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    document.title = 'Admin - Stories';
    loadStories();
  }, []);

  const loadStories = async () => {
    const data = await api.getStories();
    setStories(data);
  };

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newTitle.trim()) return;
    
    const newStory = await api.createStory(newTitle, newDescription);
    navigate(`/admin/story/${newStory.id}`);
  };

  return (
    <div className="stories-list-container">
      <div className="header-actions">
        <h2>Your Stories</h2>
        <button className="btn-primary" onClick={() => setIsCreating(true)}>
          <PlusCircle size={18} /> New Story
        </button>
      </div>

      {isCreating && (
        <form className="create-story-card" onSubmit={handleCreate}>
          <h3>Create New Story</h3>
          <input 
            type="text" 
            placeholder="Story Title" 
            value={newTitle}
            onChange={(e) => setNewTitle(e.target.value)}
            autoFocus
          />
          <textarea 
            placeholder="Story Description" 
            value={newDescription}
            onChange={(e) => setNewDescription(e.target.value)}
          />
          <div className="card-actions">
            <button type="button" className="btn-secondary" onClick={() => setIsCreating(false)}>Cancel</button>
            <button type="submit" className="btn-primary">Create</button>
          </div>
        </form>
      )}

      <div className="stories-grid">
        {stories.map(story => (
          <Link to={`/admin/story/${story.id}`} key={story.id} className="story-card">
            <div className="story-card-icon">
              <Book size={32} />
            </div>
            <div className="story-card-content">
              <h3>{story.title}</h3>
              <p>{story.description}</p>
              <div 
                className="story-id" 
                style={{ marginTop: '0.5rem', fontSize: '0.8rem', color: '#666', display: 'flex', alignItems: 'center', gap: '0.5rem' }}
                onClick={(e) => {
                  e.preventDefault(); // Prevent navigating to story editor
                  navigator.clipboard.writeText(story.id);
                  // Could use a toast here, but alert is simple
                  alert('GUID copied to clipboard!');
                }}
                title="Click to copy ID"
              >
                <span>ID: {story.id}</span>
                <Copy size={14} style={{ cursor: 'pointer' }} />
              </div>
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
};

export default StoriesList;
