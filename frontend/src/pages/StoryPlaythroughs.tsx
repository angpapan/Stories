import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { api, Story, Playthrough, Node } from '../api';
import { ArrowLeft, ChevronDown, ChevronRight } from 'lucide-react';

const StoryPlaythroughs: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [story, setStory] = useState<Story | null>(null);
  const [playthroughs, setPlaythroughs] = useState<Playthrough[]>([]);
  const [nodes, setNodes] = useState<Record<string, Node>>({});
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (id) {
      loadData(id);
    }
  }, [id]);

  useEffect(() => {
    if (story) {
      document.title = story.title ? `${story.title} - Playthroughs` : 'Story Playthroughs';
    } else {
      document.title = 'Story Playthroughs';
    }
  }, [story]);

  const loadData = async (storyId: string) => {
    setLoading(true);
    const stories = await api.getStories();
    const st = stories.find(s => s.id === storyId);
    if (st) setStory(st);

    const pts = await api.getPlaythroughs(storyId);
    setPlaythroughs(pts);

    const nds = await api.getStoryNodes(storyId);
    const nodeMap: Record<string, Node> = {};
    nds.forEach(n => {
      nodeMap[n.id] = n;
    });
    setNodes(nodeMap);
    setLoading(false);
  };

  const toggleExpand = (pid: string) => {
    setExpandedId(expandedId === pid ? null : pid);
  };

  if (loading) return <div className="loading">Loading playthroughs...</div>;
  if (!story) return <div className="loading">Story not found.</div>;

  return (
    <div className="story-editor-container">
      <div className="story-editor-header">
        <Link to={`/admin/story/${story.id}`} className="back-link">
          <ArrowLeft size={18} /> Back to Editor
        </Link>
        <div className="title-section">
          <h2>Playthroughs: {story.title}</h2>
          <p>View player choices and endings</p>
        </div>
      </div>
      
      <div className="playthroughs-content" style={{ padding: '2rem', overflowY: 'auto' }}>
        {playthroughs.length === 0 ? (
          <div className="empty-state">No playthroughs yet for this story.</div>
        ) : (
          <div className="playthroughs-list" style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
            {playthroughs.map(p => (
              <div key={p.id} className="playthrough-card" style={{ border: '1px solid #333', borderRadius: '8px', overflow: 'hidden' }}>
                <div 
                  className="playthrough-header" 
                  onClick={() => toggleExpand(p.id)}
                  style={{ display: 'flex', justifyContent: 'space-between', padding: '1rem', background: '#222', cursor: 'pointer', alignItems: 'center' }}
                >
                  <div>
                    <strong>ID:</strong> {p.id.substring(0, 8)}... | 
                    <strong> Status:</strong> <span style={{ color: p.status === 1 ? '#4ade80' : '#facc15' }}>{p.status === 1 ? 'Completed' : 'In Progress'}</span> |
                    <strong> Date:</strong> {new Date(p.createdAt).toLocaleString()}
                  </div>
                  <div>
                    {expandedId === p.id ? <ChevronDown size={20} /> : <ChevronRight size={20} />}
                  </div>
                </div>
                {expandedId === p.id && (
                  <div className="playthrough-details" style={{ padding: '1rem', background: '#111' }}>
                    <h4 style={{ marginBottom: '0.5rem', borderBottom: '1px solid #333', paddingBottom: '0.5rem' }}>History ({p.history.length} steps)</h4>
                    <ul style={{ listStyle: 'none', padding: 0, margin: 0, display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                      {p.history.map((h, i) => {
                        const node = nodes[h.nodeId];
                        const choice = node?.choices.find(c => c.id === h.chosenChoiceId);
                        
                        return (
                          <li key={i} style={{ borderLeft: '2px solid #555', paddingLeft: '1rem' }}>
                            <div style={{ color: '#aaa', fontSize: '0.85rem', marginBottom: '0.2rem' }}>
                              Step {h.sequenceNumber + 1} - {new Date(h.answeredAt).toLocaleTimeString()}
                            </div>
                            <div style={{ fontWeight: '500' }}>
                              Node: <span style={{ color: '#fff' }}>{node?.text || h.nodeId}</span>
                            </div>
                            {h.chosenChoiceId && (
                              <div style={{ marginTop: '0.25rem', color: '#60a5fa' }}>
                                ↳ Chose: "{choice?.text || h.chosenChoiceId}"
                              </div>
                            )}
                          </li>
                        );
                      })}
                    </ul>
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default StoryPlaythroughs;
