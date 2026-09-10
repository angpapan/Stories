import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import DOMPurify from 'dompurify';
import { playerApi, PlayerNode, StoryInfoResponse } from '../playerApi';
import { config } from '../config';
import './Playthrough.css';

const Playthrough: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [storyInfo, setStoryInfo] = useState<StoryInfoResponse | null>(null);
  const [password, setPassword] = useState('');
  const [resumeId, setResumeId] = useState('');

  const [playthroughId, setPlaythroughId] = useState<string | null>(null);
  const [currentNode, setCurrentNode] = useState<PlayerNode | null>(null);
  const [status, setStatus] = useState<'InProgress' | 'Completed'>('InProgress');
  const [toastMsg, setToastMsg] = useState<string | null>(null);

  const showToast = (msg: string) => {
    setToastMsg(msg);
    setTimeout(() => setToastMsg(null), 2500);
  };

  useEffect(() => {
    if (!id) return;
    const fetchInfo = async () => {
      setLoading(true);
      const res = await playerApi.getStoryInfo(id);
      
      if ('error' in res) {
        setError(res.error);
        setLoading(false);
      } else {
        if (res.title) {
          document.title = res.title;
        }

        // Check if we have a cached password
        const cachedPassword = localStorage.getItem(`story_password_${id}`);
        if (res.requiresPassword && cachedPassword) {
          const unlockRes = await playerApi.unlockStory(id, cachedPassword);
          if (!('error' in unlockRes)) {
            setStoryInfo({ ...unlockRes, requiresPassword: false });
            setPassword(cachedPassword);
            setLoading(false);
            return;
          } else {
            // Password invalid now, clear it
            localStorage.removeItem(`story_password_${id}`);
          }
        }
        
        setStoryInfo(res);
        setLoading(false);
      }
    };
    fetchInfo();
  }, [id]);

  const handleProceed = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id) return;
    setLoading(true);
    setError(null);
    const res = await playerApi.unlockStory(id, password);
    setLoading(false);

    if ('error' in res) {
      if (res.status === 401) {
        setError('Incorrect password.');
      } else {
        setError(res.error);
      }
      return;
    }

    localStorage.setItem(`story_password_${id}`, password);
    setStoryInfo({ ...res, requiresPassword: false });
  };

  const handleBegin = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id) return;
    setLoading(true);
    setError(null);
    const res = await playerApi.startPlaythrough(id, password);
    setLoading(false);

    if ('error' in res) {
      if (res.status === 401) {
        setError('Incorrect password.');
      } else {
        setError(res.error);
      }
      return;
    }

    setPlaythroughId(res.playthroughId);
    setCurrentNode(res.node);
    setStatus(res.status);
  };

  const handleResume = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!resumeId.trim()) return;
    
    setLoading(true);
    setError(null);
    
    const res = await playerApi.resumePlaythrough(resumeId.trim());
    setLoading(false);
    
    if ('error' in res) {
      setError('Could not resume: ' + res.error);
      return;
    }
    
    setPlaythroughId(resumeId.trim());
    setCurrentNode(res.node);
    setStatus(res.status);
  };

  const handleChoice = async (choiceId: string) => {
    if (!playthroughId) return;
    setLoading(true);
    setError(null);
    const res = await playerApi.makeChoice(playthroughId, choiceId);
    setLoading(false);

    if ('error' in res) {
      setError(res.error);
      return;
    }

    setCurrentNode(res.node);
    setStatus(res.status);
  };

  if (loading && !storyInfo && !currentNode) {
    return <div className="playthrough-container">Loading story...</div>;
  }

  if (error && !currentNode && !storyInfo) {
    return (
      <div className="playthrough-container error-screen">
        <h2>Oops!</h2>
        <p>{error}</p>
        <button onClick={() => navigate('/')}>Go Home</button>
      </div>
    );
  }

  if (!currentNode && storyInfo) {
    return (
      <div className="playthrough-container landing-screen">
        {!storyInfo.requiresPassword && (
          <>
            {config.ALLOW_HTML_IN_STORY ? (
              <h2 dangerouslySetInnerHTML={{ __html: DOMPurify.sanitize(storyInfo.title || '') }} />
            ) : (
              <h2>{storyInfo.title}</h2>
            )}
            
            {config.ALLOW_HTML_IN_STORY && storyInfo.description ? (
              <p dangerouslySetInnerHTML={{ __html: DOMPurify.sanitize(storyInfo.description) }} />
            ) : (
              <p>{storyInfo.description}</p>
            )}
          </>
        )}
        
        {storyInfo.requiresPassword && (
          <h2>Password Required</h2>
        )}

        {error && <div className="error-message">{error}</div>}
        
        <form onSubmit={storyInfo.requiresPassword ? handleProceed : handleBegin}>
          {storyInfo.requiresPassword ? (
            <>
              {storyInfo.passwordHint && (
                <p className="password-hint" style={{ fontSize: '0.8rem', color: '#aaa', marginBottom: '1rem' }}>
                  Hint: {storyInfo.passwordHint}
                </p>
              )}
              <input
                type="password"
                placeholder="Enter password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
              <button type="submit" disabled={loading}>
                {loading ? 'Checking...' : 'Proceed'}
              </button>
            </>
          ) : (
            <button type="submit" disabled={loading}>
              {loading ? 'Starting...' : 'Begin'}
            </button>
          )}
        </form>
        
        {!storyInfo.requiresPassword && (
          <form className="resume-form" onSubmit={handleResume} style={{ marginTop: '2rem', paddingTop: '2rem', borderTop: '1px solid #444' }}>
            <h3>Resume Playthrough</h3>
            <p style={{ fontSize: '0.85rem', color: '#888', marginBottom: '1rem' }}>Have a playthrough ID? Enter it below to pick up where you left off.</p>
            <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'center' }}>
              <input
                type="text"
                placeholder="Playthrough ID"
                value={resumeId}
                onChange={(e) => setResumeId(e.target.value)}
                required
                style={{ padding: '0.5rem', borderRadius: '4px', border: '1px solid #555', background: '#222', color: '#fff' }}
              />
              <button type="submit" disabled={loading} className="btn-secondary">
                {loading ? 'Resuming...' : 'Continue'}
              </button>
            </div>
          </form>
        )}
      </div>
    );
  }

  if (!currentNode) {
    return null;
  }

  return (
    <div className="playthrough-container">
      {playthroughId && (
        <div className="playthrough-id-banner" style={{ background: '#2a2a2a', padding: '0.5rem 1rem', fontSize: '0.8rem', color: '#aaa', display: 'flex', justifyContent: 'space-between', alignItems: 'center', borderBottom: '1px solid #444' }}>
          <span>Save this ID to resume your game later: <strong>{playthroughId}</strong></span>
          <button 
            onClick={() => { navigator.clipboard.writeText(playthroughId); showToast('Copied to clipboard!'); }}
            style={{ background: 'none', border: '1px solid #555', color: '#ddd', padding: '2px 8px', borderRadius: '4px', cursor: 'pointer', fontSize: '0.75rem' }}
          >
            Copy
          </button>
        </div>
      )}
      
      {error && <div className="error-toast">{error}</div>}

      <div className="story-node">
        {currentNode.media?.map((m, idx) => (
          <div key={idx} className="story-media">
            {m.type === 'image' && <img src={m.url} alt={m.caption} />}
          </div>
        ))}
        {config.ALLOW_HTML_IN_STORY ? (
          <p 
            className="story-text" 
            dangerouslySetInnerHTML={{ __html: DOMPurify.sanitize(currentNode.text) }} 
          />
        ) : (
          <p className="story-text">{currentNode.text}</p>
        )}
      </div>

      <div className="choices-container">
        {status === 'InProgress' && currentNode.choices?.map(choice => (
          <button
            key={choice.id}
            className="choice-button"
            onClick={() => handleChoice(choice.id)}
            disabled={loading}
          >
            {config.ALLOW_HTML_IN_STORY ? (
              <span dangerouslySetInnerHTML={{ __html: DOMPurify.sanitize(choice.text) }} />
            ) : (
              choice.text
            )}
          </button>
        ))}

        {(status === 'Completed' || currentNode.isEnding) && (
          <div className="story-end">
            <h3>The End</h3>
          </div>
        )}
      </div>

      {toastMsg && <div className="elegant-toast">{toastMsg}</div>}
    </div>
  );
};

export default Playthrough;
